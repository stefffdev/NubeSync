class NubeOperation {
  String? id;
  DateTime? createdAt;
  String itemId;
  String? oldValue;
  String property;
  String tableName;
  OperationType type;
  String? value;

  NubeOperation(
      {this.createdAt,
      required this.itemId,
      required this.value,
      required this.property,
      required this.tableName,
      required this.type,
      this.oldValue,
      }) {
        createdAt = DateTime.now();
      }

  Map<String, dynamic> toMap() {
    var map = <String, dynamic>{
      'createdAt': createdAt != null
          ? createdAt!.toIso8601String()
          : DateTime.now().toString(),
      'itemId': itemId,
      'oldValue': oldValue,
      'property': property,
      'tableName': tableName,
      'type': type.index,
      'value': value
    };

    if (id != null) {
      map['id'] = id;
    }

    return map;
  }

  factory NubeOperation.fromMap(Map<String, dynamic> map) {
    var operation = NubeOperation(
      createdAt: DateTime.parse(map['createdAt']), 
      itemId: map['itemId'], 
      oldValue: map['oldValue'], 
      property: map['property'], 
      tableName: map['tableName'],
      type: OperationType.values[map['type']], 
      value: map['value'], 
    );

    operation.id = map['id'];
    return operation;
  }
}

enum OperationType { added, modified, deleted }