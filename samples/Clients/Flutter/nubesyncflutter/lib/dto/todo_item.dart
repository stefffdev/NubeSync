import 'package:nubesyncflutter/_nubeSync/nube_table.dart';

class TodoItem extends NubeTable {
  final String columnId = 'id';
  final String columnName = 'name';

  @override
  String get createTableQuery => '''CREATE TABLE IF NOT EXISTS $tableName (
        $columnId varchar NOT NULL PRIMARY KEY, 
        $columnName TEXT)''';

  @override
  String get tableName => 'TodoItem';

  @override
  String get tableUrl => 'api/todoitems';

  String name;

  TodoItem({id, this.name = ''}) : super(id: id);

  @override
  Map<String, dynamic> toMap() {
    var map = <String, dynamic>{
      columnName: name,
    };

    if (id != null) {
      map[columnId] = id;
    }

    return map;
  }

  TodoItem fromMap(Map<String, dynamic> map) {
    return TodoItem(id: map[columnId], name: map[columnName]);
  }
}
