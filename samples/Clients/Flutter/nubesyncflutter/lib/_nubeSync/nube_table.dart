abstract class NubeTable {
  final String tableName;
  final String tableUrl;
  final String createTableQuery;

  String? id;
  DateTime? createdAt;
  DateTime? updatedAt;

  NubeTable(
      {this.id, this.tableName = '', this.tableUrl = '', this.createTableQuery = ''});

  Map<String, dynamic> toMap();

  dynamic fromMap(Map<String, dynamic> map);
}
