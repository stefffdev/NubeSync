import 'package:nubesyncflutter/_nubeSync/nube_client.dart';
import 'package:nubesyncflutter/_nubeSync/nube_operation.dart';
import 'package:nubesyncflutter/_nubeSync/nube_table.dart';

class ChangeTracker {
  ChangeTracker._privateConstructor();
  static final ChangeTracker instance = ChangeTracker._privateConstructor();

  Future<void> trackAdd(NubeTable item) async {
    if (item.id != null && item.id!.isEmpty) {
      throw ('cannot track add for empty id');
    }

    var type = item.runtimeType.toString();
    var operations = <NubeOperation>[];
    var operation = NubeOperation(
        tableName: item.tableName,
        itemId: item.id!,
        type: OperationType.added,
        property: '',
        value: '');
    operations.add(operation);

    var properties = item.toMap();
    for (var key in properties.keys) {
      var val = _convertValue(properties[key]);

      if (key is String && val != null && val.isEmpty) {
        print('skipping $key because it is empty');
      } else {
        operations.add(NubeOperation(
            tableName: type,
            itemId: item.id!,
            type: OperationType.modified,
            property: key,
            value: _convertValue(properties[key]),
            createdAt: item.updatedAt));
      }
    }

    await NubeClient.instance.addOperations(operations);
  }

  Future<void> trackModify(NubeTable oldItem, NubeTable newItem) async {
    if (newItem.id != null && newItem.id!.isEmpty) {
      throw ('cannot track modify for newItem with empty id');
    }

    if (oldItem.runtimeType != newItem.runtimeType) {
      throw ('cannot compare objects of different type');
    }

    if (oldItem.id != newItem.id) {
      throw ('cannot compare different records');
    }

    var oldProperties = oldItem.toMap();
    var newProperties = newItem.toMap();
    var tableName = newItem.tableName;
    var operations = <NubeOperation>[];
    var obsoleteOperations = <NubeOperation>[];

    for (var key in newProperties.keys) {
      var oldPropertyValue = oldProperties[key];
      if (oldPropertyValue != newProperties[key] && key != 'id') {
        operations.add(NubeOperation(
            tableName: tableName,
            itemId: newItem.id!,
            type: OperationType.modified,
            property: key,
            value: newProperties[key],
            oldValue: oldPropertyValue));

        // cleanup obsolete operations
        obsoleteOperations.addAll((await NubeClient.instance.getOperations())
            .where((o) =>
                o.itemId == newItem.id &&
                o.property == key &&
                o.type == OperationType.modified));
      }

      if (operations.isEmpty) {
        return;
      }

      await NubeClient.instance.addOperations(operations);
      await NubeClient.instance.deleteOperations(obsoleteOperations);
    }
  }

  Future<void> trackDelete(NubeTable item) async {
    if (item.id!.isEmpty) {
      throw ('cannot track delete for item with empty id');
    }

    var operations = <NubeOperation>[];
    operations.add(NubeOperation(
        tableName: item.tableName,
        itemId: item.id!,
        value: '',
        property: '',
        type: OperationType.deleted));
    await NubeClient.instance.addOperations(operations);

    // cleanup obsolete operations
    var obsoleteOperations = (await NubeClient.instance.getOperations())
        .where((o) => o.itemId == item.id && o.type != OperationType.deleted);
    await NubeClient.instance.deleteOperations(obsoleteOperations.toList());
  }

  String? _convertValue(dynamic value) {
    if (value is DateTime) {
      return value.toIso8601String();
    }

    return value != null ? value.toString() : null;
  }
}
