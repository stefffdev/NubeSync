import 'dart:convert';
import 'dart:io';
import 'package:nubesyncflutter/_nubeSync/authentication.dart';
import 'package:nubesyncflutter/_nubeSync/change_tracker.dart';
import 'package:nubesyncflutter/_nubeSync/nube_operation.dart';
import 'package:nubesyncflutter/_nubeSync/nube_table.dart';
import 'package:sqflite/sqflite.dart';
import 'package:uuid/uuid.dart';
import 'package:http/http.dart' as http;

class NubeClient {
  final String _operationTable = '__operations';
  final String _settingsTable = '__settings';
  final String _installationIdHeader = 'NUBE-INSTALLATION-ID';
  final int _operationsPageSize = 100;
  String _installationId = 'installationId';
  bool _isSyncing = false;

  Map<String, Function()> _factories = <String, Function()>{};

  Database? db;
  String serverUrl = '';
  String databaseName = 'nubecache5.db';

  NubeClient._privateConstructor();
  static final NubeClient instance = NubeClient._privateConstructor();

  Future<void> initialize() async {
    if (db != null) {
      return;
    }

    db = await openDatabase(databaseName, onConfigure: (Database db) async {
      await db.execute('''CREATE TABLE IF NOT EXISTS $_operationTable
          (id varchar NOT NULL PRIMARY KEY,
          createdAt TEXT,
          itemId TEXT,
          oldValue TEXT,
          property TEXT,
          tableName TEXT,
          type INTEGER,
          value TEXT)''');

      await db.execute('''CREATE TABLE IF NOT EXISTS $_settingsTable
          (id varchar NOT NULL PRIMARY KEY,
          value TEXT)''');
    });

    _installationId = await _getInstallationId();
  }

  Future<void> addTable<T extends NubeTable>(Function function) async {
    await initialize();

    if (!_factories.containsKey('$T')) {
      _factories['$T'] = function as dynamic;
      await db!.execute(function().createTableQuery);
    }
  }

  void isValidTable<T extends NubeTable>() {
    if (_factories == null || !_factories.containsKey('$T')) {
      throw ('Table of type $T is not registered in the nube client');
    }
  }

  Future<String> _getInstallationId() async {
    final String installationId = 'installationId';
    var id = await getSetting(installationId);
    if (id == null || id.isEmpty) {
      id = Uuid().v4();
      await setSetting(installationId, id);
    }

    return id;
  }

  // Database Access
  Future<List<T>> getAll<T extends NubeTable>() async {
    isValidTable<T>();

    var instance = _factories['$T']!();
    var data = await db!.query(instance.tableName);

    var result = <T>[];
    data.forEach((element) {
      result.add(instance.fromMap(element));
    });

    return result;
  }

  Future<T?> getById<T extends NubeTable>(String? id) async {
    if (id == null || id.isEmpty) {
      return null;
    }

    isValidTable<T>();

    var instance = _factories['$T']!();
    List<Map> maps =
        await db!.query(instance.tableName, where: 'id = ?', whereArgs: [id]);

    if (maps.length > 0) {
      return instance.fromMap(maps.first);
    }

    return null;
  }

  Future<List<T>> query<T extends NubeTable>(String query) async {
    isValidTable<T>();

    var instance = _factories['$T']!();
    var data = await db!.rawQuery(query);

    var result = <T>[];
    data.forEach((element) {
      result.add(instance.fromMap(element));
    });

    return result;
  }

  Future<bool> delete(NubeTable item,
      {bool disableChangeTracker = false}) async {
    if (item == null) {
      return true;
    }

    if (await db!
            .delete(item.tableName, where: 'id = ?', whereArgs: [item.id]) >
        0) {
      if (!disableChangeTracker) {
        await ChangeTracker.instance.trackDelete(item);
      }
      return true;
    }

    return false;
  }

  Future<bool> save<T extends NubeTable>(T item,
      {bool disableChangeTracker = false}) async {
    isValidTable<T>();

    var now = DateTime.now();
    if (!disableChangeTracker) {
      item.updatedAt = now;
    }

    var existingItem = await getById<T>(item.id);
    if (existingItem == null) {
      if (item.id == null || item.id!.isEmpty) {
        item.id = Uuid().v4();
      }
      if (!disableChangeTracker) {
        item.createdAt = now;
      }
      if (await db!.insert(item.tableName, item.toMap()) > 0) {
        if (!disableChangeTracker) {
          ChangeTracker.instance.trackAdd(item);
        }
        return true;
      } else {
        throw ('Could not insert item $T');
      }
    } else {
      if (await db!.update(item.tableName, item.toMap(),
              where: 'id = ?', whereArgs: [item.id]) >
          0) {
        await ChangeTracker.instance.trackModify(existingItem, item);
        return true;
      } else {
        throw ('Could not update item $T');
      }
    }
  }

  // Operations
  Future<void> addOperations(List<NubeOperation> operations) async {
    for (var element in operations) {
      element.id = Uuid().v4();
      await db!.insert(_operationTable, element.toMap());
    }
  }

  Future<void> deleteOperations(List<NubeOperation> operations) async {
    for (var element in operations) {
      await db!
          .delete(_operationTable, where: 'id = ?', whereArgs: [element.id]);
    }
  }

  Future<List<NubeOperation>> getOperations({numberOfOperations = 0}) async {
    var result = <NubeOperation>[];

    if (numberOfOperations == 0) {
      numberOfOperations = null;
    }

    var data = await db!.query(_operationTable,
        orderBy: 'createdAt', limit: numberOfOperations);
    data.forEach((element) {
      result.add(NubeOperation.fromMap(element));
    });

    return result;
  }

  // Settings
  Future<String> getSetting(String key) async {
    List<Map> maps = await db!.query(_settingsTable,
        columns: ['value'], where: 'id = ?', whereArgs: [key]);
    if (maps.length > 0) {
      return maps.first['value'];
    }
    return '';
  }

  Future<bool> setSetting(String key, String value) async {
    int updatedRecords = 0;
    var currentSetting = await getSetting(key);
    if (currentSetting == null) {
      updatedRecords = await db!.rawInsert(
          'INSERT INTO $_settingsTable(id, value) VALUES("$key", "$value")');
    } else {
      updatedRecords = await db!.rawUpdate(
          'UPDATE $_settingsTable SET value = ? WHERE id = ?',
          ['$value', '$key']);
    }

    return updatedRecords > 0;
  }

  // Sync
  Future<int> pushChanges() async {
    int result = 0;
    if (_isSyncing) {
      return result;
    }
    _isSyncing = true;

    try {
      var operations =
          await getOperations(numberOfOperations: _operationsPageSize);

      while (operations.isNotEmpty) {
        List jsonList = [];
        operations.map((item) => jsonList.add(item.toMap())).toList();

        var requestBody = json.encoder.convert(jsonList);
        var token = await getAccessToken();
        var response = await http.post(
          Uri.parse('$serverUrl/api/operations'),
          headers: {
            HttpHeaders.authorizationHeader: 'Bearer $token',
            HttpHeaders.contentTypeHeader: 'application/json',
            _installationIdHeader: _installationId
          },
          body: requestBody,
        );

        if (response.statusCode == 200) {
          await deleteOperations(operations);
          result += operations.length;
        } else {
          throw Exception(
              'Could not push changes: ${response.statusCode} ${response.reasonPhrase} ${response.body}');
        }

        operations =
            await getOperations(numberOfOperations: _operationsPageSize);
      }
    } finally {
      _isSyncing = false;
    }

    return result;
  }

  Future<int> pullTable<T extends NubeTable>() async {
    int result = 0;

    if (_isSyncing) {
      return result;
    }
    _isSyncing = true;

    try {
      isValidTable<T>();

      var instance = _factories['$T']!();
      var token = await getAccessToken();

      String parameters = '';
      var lastSync = await getSetting('lastSync-${instance.tableName}');
      if (lastSync.isNotEmpty) {
        parameters = '?laterThan=$lastSync';
      }

      var uri = Uri.parse('$serverUrl/${instance.tableUrl.trim()}$parameters');
      var response = await http.get(
        uri,
        headers: {
          HttpHeaders.authorizationHeader: 'Bearer $token',
          _installationIdHeader: _installationId
        },
      );

      if (response.statusCode == 200) {
        var jsonList = jsonDecode(response.body) as List;

        for (var element in jsonList) {
          var item = instance.fromMap(element);

          if (element['deletedAt'] != null) {
            var localItem = await getById<T>(item.id);
            if (localItem != null)
              await delete(localItem, disableChangeTracker: true);
          } else {
            await save<T>(item, disableChangeTracker: true);
          }
        }

        result = jsonList.length;
        await setSetting('lastSync-${instance.tableName}',
            DateTime.now().toUtc().toIso8601String());
      } else {
        throw Exception(
            'Could not load $T: ${response.statusCode} ${response.reasonPhrase}');
      }
    } finally {
      _isSyncing = false;
    }

    return result;
  }
}
