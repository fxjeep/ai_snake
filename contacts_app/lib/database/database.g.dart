// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'database.dart';

// ignore_for_file: type=lint
class $ContactsTable extends Contacts with TableInfo<$ContactsTable, Contact> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $ContactsTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<int> id = GeneratedColumn<int>(
    'id',
    aliasedName,
    false,
    hasAutoIncrement: true,
    type: DriftSqlType.int,
    requiredDuringInsert: false,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'PRIMARY KEY AUTOINCREMENT',
    ),
  );
  static const VerificationMeta _nameMeta = const VerificationMeta('name');
  @override
  late final GeneratedColumn<String> name = GeneratedColumn<String>(
    'name',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _codeMeta = const VerificationMeta('code');
  @override
  late final GeneratedColumn<String> code = GeneratedColumn<String>(
    'code',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _lastPrintDateMeta = const VerificationMeta(
    'lastPrintDate',
  );
  @override
  late final GeneratedColumn<DateTime> lastPrintDate =
      GeneratedColumn<DateTime>(
        'last_print_date',
        aliasedName,
        true,
        type: DriftSqlType.dateTime,
        requiredDuringInsert: false,
      );
  static const VerificationMeta _initialsMeta = const VerificationMeta(
    'initials',
  );
  @override
  late final GeneratedColumn<String> initials = GeneratedColumn<String>(
    'initials',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _profileImageMeta = const VerificationMeta(
    'profileImage',
  );
  @override
  late final GeneratedColumn<String> profileImage = GeneratedColumn<String>(
    'profile_image',
    aliasedName,
    true,
    type: DriftSqlType.string,
    requiredDuringInsert: false,
  );
  @override
  List<GeneratedColumn> get $columns => [
    id,
    name,
    code,
    lastPrintDate,
    initials,
    profileImage,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'contacts';
  @override
  VerificationContext validateIntegrity(
    Insertable<Contact> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    }
    if (data.containsKey('name')) {
      context.handle(
        _nameMeta,
        name.isAcceptableOrUnknown(data['name']!, _nameMeta),
      );
    } else if (isInserting) {
      context.missing(_nameMeta);
    }
    if (data.containsKey('code')) {
      context.handle(
        _codeMeta,
        code.isAcceptableOrUnknown(data['code']!, _codeMeta),
      );
    } else if (isInserting) {
      context.missing(_codeMeta);
    }
    if (data.containsKey('last_print_date')) {
      context.handle(
        _lastPrintDateMeta,
        lastPrintDate.isAcceptableOrUnknown(
          data['last_print_date']!,
          _lastPrintDateMeta,
        ),
      );
    }
    if (data.containsKey('initials')) {
      context.handle(
        _initialsMeta,
        initials.isAcceptableOrUnknown(data['initials']!, _initialsMeta),
      );
    } else if (isInserting) {
      context.missing(_initialsMeta);
    }
    if (data.containsKey('profile_image')) {
      context.handle(
        _profileImageMeta,
        profileImage.isAcceptableOrUnknown(
          data['profile_image']!,
          _profileImageMeta,
        ),
      );
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  Contact map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return Contact(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}id'],
      )!,
      name: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name'],
      )!,
      code: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}code'],
      )!,
      lastPrintDate: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}last_print_date'],
      ),
      initials: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}initials'],
      )!,
      profileImage: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}profile_image'],
      ),
    );
  }

  @override
  $ContactsTable createAlias(String alias) {
    return $ContactsTable(attachedDatabase, alias);
  }
}

class Contact extends DataClass implements Insertable<Contact> {
  final int id;
  final String name;
  final String code;
  final DateTime? lastPrintDate;
  final String initials;
  final String? profileImage;
  const Contact({
    required this.id,
    required this.name,
    required this.code,
    this.lastPrintDate,
    required this.initials,
    this.profileImage,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<int>(id);
    map['name'] = Variable<String>(name);
    map['code'] = Variable<String>(code);
    if (!nullToAbsent || lastPrintDate != null) {
      map['last_print_date'] = Variable<DateTime>(lastPrintDate);
    }
    map['initials'] = Variable<String>(initials);
    if (!nullToAbsent || profileImage != null) {
      map['profile_image'] = Variable<String>(profileImage);
    }
    return map;
  }

  ContactsCompanion toCompanion(bool nullToAbsent) {
    return ContactsCompanion(
      id: Value(id),
      name: Value(name),
      code: Value(code),
      lastPrintDate: lastPrintDate == null && nullToAbsent
          ? const Value.absent()
          : Value(lastPrintDate),
      initials: Value(initials),
      profileImage: profileImage == null && nullToAbsent
          ? const Value.absent()
          : Value(profileImage),
    );
  }

  factory Contact.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return Contact(
      id: serializer.fromJson<int>(json['id']),
      name: serializer.fromJson<String>(json['name']),
      code: serializer.fromJson<String>(json['code']),
      lastPrintDate: serializer.fromJson<DateTime?>(json['lastPrintDate']),
      initials: serializer.fromJson<String>(json['initials']),
      profileImage: serializer.fromJson<String?>(json['profileImage']),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<int>(id),
      'name': serializer.toJson<String>(name),
      'code': serializer.toJson<String>(code),
      'lastPrintDate': serializer.toJson<DateTime?>(lastPrintDate),
      'initials': serializer.toJson<String>(initials),
      'profileImage': serializer.toJson<String?>(profileImage),
    };
  }

  Contact copyWith({
    int? id,
    String? name,
    String? code,
    Value<DateTime?> lastPrintDate = const Value.absent(),
    String? initials,
    Value<String?> profileImage = const Value.absent(),
  }) => Contact(
    id: id ?? this.id,
    name: name ?? this.name,
    code: code ?? this.code,
    lastPrintDate: lastPrintDate.present
        ? lastPrintDate.value
        : this.lastPrintDate,
    initials: initials ?? this.initials,
    profileImage: profileImage.present ? profileImage.value : this.profileImage,
  );
  Contact copyWithCompanion(ContactsCompanion data) {
    return Contact(
      id: data.id.present ? data.id.value : this.id,
      name: data.name.present ? data.name.value : this.name,
      code: data.code.present ? data.code.value : this.code,
      lastPrintDate: data.lastPrintDate.present
          ? data.lastPrintDate.value
          : this.lastPrintDate,
      initials: data.initials.present ? data.initials.value : this.initials,
      profileImage: data.profileImage.present
          ? data.profileImage.value
          : this.profileImage,
    );
  }

  @override
  String toString() {
    return (StringBuffer('Contact(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('code: $code, ')
          ..write('lastPrintDate: $lastPrintDate, ')
          ..write('initials: $initials, ')
          ..write('profileImage: $profileImage')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode =>
      Object.hash(id, name, code, lastPrintDate, initials, profileImage);
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is Contact &&
          other.id == this.id &&
          other.name == this.name &&
          other.code == this.code &&
          other.lastPrintDate == this.lastPrintDate &&
          other.initials == this.initials &&
          other.profileImage == this.profileImage);
}

class ContactsCompanion extends UpdateCompanion<Contact> {
  final Value<int> id;
  final Value<String> name;
  final Value<String> code;
  final Value<DateTime?> lastPrintDate;
  final Value<String> initials;
  final Value<String?> profileImage;
  const ContactsCompanion({
    this.id = const Value.absent(),
    this.name = const Value.absent(),
    this.code = const Value.absent(),
    this.lastPrintDate = const Value.absent(),
    this.initials = const Value.absent(),
    this.profileImage = const Value.absent(),
  });
  ContactsCompanion.insert({
    this.id = const Value.absent(),
    required String name,
    required String code,
    this.lastPrintDate = const Value.absent(),
    required String initials,
    this.profileImage = const Value.absent(),
  }) : name = Value(name),
       code = Value(code),
       initials = Value(initials);
  static Insertable<Contact> custom({
    Expression<int>? id,
    Expression<String>? name,
    Expression<String>? code,
    Expression<DateTime>? lastPrintDate,
    Expression<String>? initials,
    Expression<String>? profileImage,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (name != null) 'name': name,
      if (code != null) 'code': code,
      if (lastPrintDate != null) 'last_print_date': lastPrintDate,
      if (initials != null) 'initials': initials,
      if (profileImage != null) 'profile_image': profileImage,
    });
  }

  ContactsCompanion copyWith({
    Value<int>? id,
    Value<String>? name,
    Value<String>? code,
    Value<DateTime?>? lastPrintDate,
    Value<String>? initials,
    Value<String?>? profileImage,
  }) {
    return ContactsCompanion(
      id: id ?? this.id,
      name: name ?? this.name,
      code: code ?? this.code,
      lastPrintDate: lastPrintDate ?? this.lastPrintDate,
      initials: initials ?? this.initials,
      profileImage: profileImage ?? this.profileImage,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<int>(id.value);
    }
    if (name.present) {
      map['name'] = Variable<String>(name.value);
    }
    if (code.present) {
      map['code'] = Variable<String>(code.value);
    }
    if (lastPrintDate.present) {
      map['last_print_date'] = Variable<DateTime>(lastPrintDate.value);
    }
    if (initials.present) {
      map['initials'] = Variable<String>(initials.value);
    }
    if (profileImage.present) {
      map['profile_image'] = Variable<String>(profileImage.value);
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('ContactsCompanion(')
          ..write('id: $id, ')
          ..write('name: $name, ')
          ..write('code: $code, ')
          ..write('lastPrintDate: $lastPrintDate, ')
          ..write('initials: $initials, ')
          ..write('profileImage: $profileImage')
          ..write(')'))
        .toString();
  }
}

class $ContactDetailsTable extends ContactDetails
    with TableInfo<$ContactDetailsTable, ContactDetail> {
  @override
  final GeneratedDatabase attachedDatabase;
  final String? _alias;
  $ContactDetailsTable(this.attachedDatabase, [this._alias]);
  static const VerificationMeta _idMeta = const VerificationMeta('id');
  @override
  late final GeneratedColumn<int> id = GeneratedColumn<int>(
    'id',
    aliasedName,
    false,
    hasAutoIncrement: true,
    type: DriftSqlType.int,
    requiredDuringInsert: false,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'PRIMARY KEY AUTOINCREMENT',
    ),
  );
  static const VerificationMeta _contactIdMeta = const VerificationMeta(
    'contactId',
  );
  @override
  late final GeneratedColumn<int> contactId = GeneratedColumn<int>(
    'contact_id',
    aliasedName,
    false,
    type: DriftSqlType.int,
    requiredDuringInsert: true,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'REFERENCES contacts (id)',
    ),
  );
  static const VerificationMeta _isPrintedMeta = const VerificationMeta(
    'isPrinted',
  );
  @override
  late final GeneratedColumn<bool> isPrinted = GeneratedColumn<bool>(
    'is_printed',
    aliasedName,
    false,
    type: DriftSqlType.bool,
    requiredDuringInsert: false,
    defaultConstraints: GeneratedColumn.constraintIsAlways(
      'CHECK ("is_printed" IN (0, 1))',
    ),
    defaultValue: const Constant(false),
  );
  static const VerificationMeta _lastPrintMeta = const VerificationMeta(
    'lastPrint',
  );
  @override
  late final GeneratedColumn<DateTime> lastPrint = GeneratedColumn<DateTime>(
    'last_print',
    aliasedName,
    true,
    type: DriftSqlType.dateTime,
    requiredDuringInsert: false,
  );
  static const VerificationMeta _name1Meta = const VerificationMeta('name1');
  @override
  late final GeneratedColumn<String> name1 = GeneratedColumn<String>(
    'name1',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _name2Meta = const VerificationMeta('name2');
  @override
  late final GeneratedColumn<String> name2 = GeneratedColumn<String>(
    'name2',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  static const VerificationMeta _name3Meta = const VerificationMeta('name3');
  @override
  late final GeneratedColumn<String> name3 = GeneratedColumn<String>(
    'name3',
    aliasedName,
    false,
    type: DriftSqlType.string,
    requiredDuringInsert: true,
  );
  @override
  late final GeneratedColumnWithTypeConverter<ContactType, String> type =
      GeneratedColumn<String>(
        'type',
        aliasedName,
        false,
        type: DriftSqlType.string,
        requiredDuringInsert: true,
      ).withConverter<ContactType>($ContactDetailsTable.$convertertype);
  @override
  List<GeneratedColumn> get $columns => [
    id,
    contactId,
    isPrinted,
    lastPrint,
    name1,
    name2,
    name3,
    type,
  ];
  @override
  String get aliasedName => _alias ?? actualTableName;
  @override
  String get actualTableName => $name;
  static const String $name = 'contact_details';
  @override
  VerificationContext validateIntegrity(
    Insertable<ContactDetail> instance, {
    bool isInserting = false,
  }) {
    final context = VerificationContext();
    final data = instance.toColumns(true);
    if (data.containsKey('id')) {
      context.handle(_idMeta, id.isAcceptableOrUnknown(data['id']!, _idMeta));
    }
    if (data.containsKey('contact_id')) {
      context.handle(
        _contactIdMeta,
        contactId.isAcceptableOrUnknown(data['contact_id']!, _contactIdMeta),
      );
    } else if (isInserting) {
      context.missing(_contactIdMeta);
    }
    if (data.containsKey('is_printed')) {
      context.handle(
        _isPrintedMeta,
        isPrinted.isAcceptableOrUnknown(data['is_printed']!, _isPrintedMeta),
      );
    }
    if (data.containsKey('last_print')) {
      context.handle(
        _lastPrintMeta,
        lastPrint.isAcceptableOrUnknown(data['last_print']!, _lastPrintMeta),
      );
    }
    if (data.containsKey('name1')) {
      context.handle(
        _name1Meta,
        name1.isAcceptableOrUnknown(data['name1']!, _name1Meta),
      );
    } else if (isInserting) {
      context.missing(_name1Meta);
    }
    if (data.containsKey('name2')) {
      context.handle(
        _name2Meta,
        name2.isAcceptableOrUnknown(data['name2']!, _name2Meta),
      );
    } else if (isInserting) {
      context.missing(_name2Meta);
    }
    if (data.containsKey('name3')) {
      context.handle(
        _name3Meta,
        name3.isAcceptableOrUnknown(data['name3']!, _name3Meta),
      );
    } else if (isInserting) {
      context.missing(_name3Meta);
    }
    return context;
  }

  @override
  Set<GeneratedColumn> get $primaryKey => {id};
  @override
  ContactDetail map(Map<String, dynamic> data, {String? tablePrefix}) {
    final effectivePrefix = tablePrefix != null ? '$tablePrefix.' : '';
    return ContactDetail(
      id: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}id'],
      )!,
      contactId: attachedDatabase.typeMapping.read(
        DriftSqlType.int,
        data['${effectivePrefix}contact_id'],
      )!,
      isPrinted: attachedDatabase.typeMapping.read(
        DriftSqlType.bool,
        data['${effectivePrefix}is_printed'],
      )!,
      lastPrint: attachedDatabase.typeMapping.read(
        DriftSqlType.dateTime,
        data['${effectivePrefix}last_print'],
      ),
      name1: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name1'],
      )!,
      name2: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name2'],
      )!,
      name3: attachedDatabase.typeMapping.read(
        DriftSqlType.string,
        data['${effectivePrefix}name3'],
      )!,
      type: $ContactDetailsTable.$convertertype.fromSql(
        attachedDatabase.typeMapping.read(
          DriftSqlType.string,
          data['${effectivePrefix}type'],
        )!,
      ),
    );
  }

  @override
  $ContactDetailsTable createAlias(String alias) {
    return $ContactDetailsTable(attachedDatabase, alias);
  }

  static JsonTypeConverter2<ContactType, String, String> $convertertype =
      const EnumNameConverter<ContactType>(ContactType.values);
}

class ContactDetail extends DataClass implements Insertable<ContactDetail> {
  final int id;
  final int contactId;
  final bool isPrinted;
  final DateTime? lastPrint;
  final String name1;
  final String name2;
  final String name3;
  final ContactType type;
  const ContactDetail({
    required this.id,
    required this.contactId,
    required this.isPrinted,
    this.lastPrint,
    required this.name1,
    required this.name2,
    required this.name3,
    required this.type,
  });
  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    map['id'] = Variable<int>(id);
    map['contact_id'] = Variable<int>(contactId);
    map['is_printed'] = Variable<bool>(isPrinted);
    if (!nullToAbsent || lastPrint != null) {
      map['last_print'] = Variable<DateTime>(lastPrint);
    }
    map['name1'] = Variable<String>(name1);
    map['name2'] = Variable<String>(name2);
    map['name3'] = Variable<String>(name3);
    {
      map['type'] = Variable<String>(
        $ContactDetailsTable.$convertertype.toSql(type),
      );
    }
    return map;
  }

  ContactDetailsCompanion toCompanion(bool nullToAbsent) {
    return ContactDetailsCompanion(
      id: Value(id),
      contactId: Value(contactId),
      isPrinted: Value(isPrinted),
      lastPrint: lastPrint == null && nullToAbsent
          ? const Value.absent()
          : Value(lastPrint),
      name1: Value(name1),
      name2: Value(name2),
      name3: Value(name3),
      type: Value(type),
    );
  }

  factory ContactDetail.fromJson(
    Map<String, dynamic> json, {
    ValueSerializer? serializer,
  }) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return ContactDetail(
      id: serializer.fromJson<int>(json['id']),
      contactId: serializer.fromJson<int>(json['contactId']),
      isPrinted: serializer.fromJson<bool>(json['isPrinted']),
      lastPrint: serializer.fromJson<DateTime?>(json['lastPrint']),
      name1: serializer.fromJson<String>(json['name1']),
      name2: serializer.fromJson<String>(json['name2']),
      name3: serializer.fromJson<String>(json['name3']),
      type: $ContactDetailsTable.$convertertype.fromJson(
        serializer.fromJson<String>(json['type']),
      ),
    );
  }
  @override
  Map<String, dynamic> toJson({ValueSerializer? serializer}) {
    serializer ??= driftRuntimeOptions.defaultSerializer;
    return <String, dynamic>{
      'id': serializer.toJson<int>(id),
      'contactId': serializer.toJson<int>(contactId),
      'isPrinted': serializer.toJson<bool>(isPrinted),
      'lastPrint': serializer.toJson<DateTime?>(lastPrint),
      'name1': serializer.toJson<String>(name1),
      'name2': serializer.toJson<String>(name2),
      'name3': serializer.toJson<String>(name3),
      'type': serializer.toJson<String>(
        $ContactDetailsTable.$convertertype.toJson(type),
      ),
    };
  }

  ContactDetail copyWith({
    int? id,
    int? contactId,
    bool? isPrinted,
    Value<DateTime?> lastPrint = const Value.absent(),
    String? name1,
    String? name2,
    String? name3,
    ContactType? type,
  }) => ContactDetail(
    id: id ?? this.id,
    contactId: contactId ?? this.contactId,
    isPrinted: isPrinted ?? this.isPrinted,
    lastPrint: lastPrint.present ? lastPrint.value : this.lastPrint,
    name1: name1 ?? this.name1,
    name2: name2 ?? this.name2,
    name3: name3 ?? this.name3,
    type: type ?? this.type,
  );
  ContactDetail copyWithCompanion(ContactDetailsCompanion data) {
    return ContactDetail(
      id: data.id.present ? data.id.value : this.id,
      contactId: data.contactId.present ? data.contactId.value : this.contactId,
      isPrinted: data.isPrinted.present ? data.isPrinted.value : this.isPrinted,
      lastPrint: data.lastPrint.present ? data.lastPrint.value : this.lastPrint,
      name1: data.name1.present ? data.name1.value : this.name1,
      name2: data.name2.present ? data.name2.value : this.name2,
      name3: data.name3.present ? data.name3.value : this.name3,
      type: data.type.present ? data.type.value : this.type,
    );
  }

  @override
  String toString() {
    return (StringBuffer('ContactDetail(')
          ..write('id: $id, ')
          ..write('contactId: $contactId, ')
          ..write('isPrinted: $isPrinted, ')
          ..write('lastPrint: $lastPrint, ')
          ..write('name1: $name1, ')
          ..write('name2: $name2, ')
          ..write('name3: $name3, ')
          ..write('type: $type')
          ..write(')'))
        .toString();
  }

  @override
  int get hashCode => Object.hash(
    id,
    contactId,
    isPrinted,
    lastPrint,
    name1,
    name2,
    name3,
    type,
  );
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      (other is ContactDetail &&
          other.id == this.id &&
          other.contactId == this.contactId &&
          other.isPrinted == this.isPrinted &&
          other.lastPrint == this.lastPrint &&
          other.name1 == this.name1 &&
          other.name2 == this.name2 &&
          other.name3 == this.name3 &&
          other.type == this.type);
}

class ContactDetailsCompanion extends UpdateCompanion<ContactDetail> {
  final Value<int> id;
  final Value<int> contactId;
  final Value<bool> isPrinted;
  final Value<DateTime?> lastPrint;
  final Value<String> name1;
  final Value<String> name2;
  final Value<String> name3;
  final Value<ContactType> type;
  const ContactDetailsCompanion({
    this.id = const Value.absent(),
    this.contactId = const Value.absent(),
    this.isPrinted = const Value.absent(),
    this.lastPrint = const Value.absent(),
    this.name1 = const Value.absent(),
    this.name2 = const Value.absent(),
    this.name3 = const Value.absent(),
    this.type = const Value.absent(),
  });
  ContactDetailsCompanion.insert({
    this.id = const Value.absent(),
    required int contactId,
    this.isPrinted = const Value.absent(),
    this.lastPrint = const Value.absent(),
    required String name1,
    required String name2,
    required String name3,
    required ContactType type,
  }) : contactId = Value(contactId),
       name1 = Value(name1),
       name2 = Value(name2),
       name3 = Value(name3),
       type = Value(type);
  static Insertable<ContactDetail> custom({
    Expression<int>? id,
    Expression<int>? contactId,
    Expression<bool>? isPrinted,
    Expression<DateTime>? lastPrint,
    Expression<String>? name1,
    Expression<String>? name2,
    Expression<String>? name3,
    Expression<String>? type,
  }) {
    return RawValuesInsertable({
      if (id != null) 'id': id,
      if (contactId != null) 'contact_id': contactId,
      if (isPrinted != null) 'is_printed': isPrinted,
      if (lastPrint != null) 'last_print': lastPrint,
      if (name1 != null) 'name1': name1,
      if (name2 != null) 'name2': name2,
      if (name3 != null) 'name3': name3,
      if (type != null) 'type': type,
    });
  }

  ContactDetailsCompanion copyWith({
    Value<int>? id,
    Value<int>? contactId,
    Value<bool>? isPrinted,
    Value<DateTime?>? lastPrint,
    Value<String>? name1,
    Value<String>? name2,
    Value<String>? name3,
    Value<ContactType>? type,
  }) {
    return ContactDetailsCompanion(
      id: id ?? this.id,
      contactId: contactId ?? this.contactId,
      isPrinted: isPrinted ?? this.isPrinted,
      lastPrint: lastPrint ?? this.lastPrint,
      name1: name1 ?? this.name1,
      name2: name2 ?? this.name2,
      name3: name3 ?? this.name3,
      type: type ?? this.type,
    );
  }

  @override
  Map<String, Expression> toColumns(bool nullToAbsent) {
    final map = <String, Expression>{};
    if (id.present) {
      map['id'] = Variable<int>(id.value);
    }
    if (contactId.present) {
      map['contact_id'] = Variable<int>(contactId.value);
    }
    if (isPrinted.present) {
      map['is_printed'] = Variable<bool>(isPrinted.value);
    }
    if (lastPrint.present) {
      map['last_print'] = Variable<DateTime>(lastPrint.value);
    }
    if (name1.present) {
      map['name1'] = Variable<String>(name1.value);
    }
    if (name2.present) {
      map['name2'] = Variable<String>(name2.value);
    }
    if (name3.present) {
      map['name3'] = Variable<String>(name3.value);
    }
    if (type.present) {
      map['type'] = Variable<String>(
        $ContactDetailsTable.$convertertype.toSql(type.value),
      );
    }
    return map;
  }

  @override
  String toString() {
    return (StringBuffer('ContactDetailsCompanion(')
          ..write('id: $id, ')
          ..write('contactId: $contactId, ')
          ..write('isPrinted: $isPrinted, ')
          ..write('lastPrint: $lastPrint, ')
          ..write('name1: $name1, ')
          ..write('name2: $name2, ')
          ..write('name3: $name3, ')
          ..write('type: $type')
          ..write(')'))
        .toString();
  }
}

abstract class _$AppDatabase extends GeneratedDatabase {
  _$AppDatabase(QueryExecutor e) : super(e);
  $AppDatabaseManager get managers => $AppDatabaseManager(this);
  late final $ContactsTable contacts = $ContactsTable(this);
  late final $ContactDetailsTable contactDetails = $ContactDetailsTable(this);
  @override
  Iterable<TableInfo<Table, Object?>> get allTables =>
      allSchemaEntities.whereType<TableInfo<Table, Object?>>();
  @override
  List<DatabaseSchemaEntity> get allSchemaEntities => [
    contacts,
    contactDetails,
  ];
}

typedef $$ContactsTableCreateCompanionBuilder =
    ContactsCompanion Function({
      Value<int> id,
      required String name,
      required String code,
      Value<DateTime?> lastPrintDate,
      required String initials,
      Value<String?> profileImage,
    });
typedef $$ContactsTableUpdateCompanionBuilder =
    ContactsCompanion Function({
      Value<int> id,
      Value<String> name,
      Value<String> code,
      Value<DateTime?> lastPrintDate,
      Value<String> initials,
      Value<String?> profileImage,
    });

final class $$ContactsTableReferences
    extends BaseReferences<_$AppDatabase, $ContactsTable, Contact> {
  $$ContactsTableReferences(super.$_db, super.$_table, super.$_typedResult);

  static MultiTypedResultKey<$ContactDetailsTable, List<ContactDetail>>
  _contactDetailsRefsTable(_$AppDatabase db) => MultiTypedResultKey.fromTable(
    db.contactDetails,
    aliasName: $_aliasNameGenerator(
      db.contacts.id,
      db.contactDetails.contactId,
    ),
  );

  $$ContactDetailsTableProcessedTableManager get contactDetailsRefs {
    final manager = $$ContactDetailsTableTableManager(
      $_db,
      $_db.contactDetails,
    ).filter((f) => f.contactId.id.sqlEquals($_itemColumn<int>('id')!));

    final cache = $_typedResult.readTableOrNull(_contactDetailsRefsTable($_db));
    return ProcessedTableManager(
      manager.$state.copyWith(prefetchedData: cache),
    );
  }
}

class $$ContactsTableFilterComposer
    extends Composer<_$AppDatabase, $ContactsTable> {
  $$ContactsTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<int> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get code => $composableBuilder(
    column: $table.code,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get lastPrintDate => $composableBuilder(
    column: $table.lastPrintDate,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get initials => $composableBuilder(
    column: $table.initials,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get profileImage => $composableBuilder(
    column: $table.profileImage,
    builder: (column) => ColumnFilters(column),
  );

  Expression<bool> contactDetailsRefs(
    Expression<bool> Function($$ContactDetailsTableFilterComposer f) f,
  ) {
    final $$ContactDetailsTableFilterComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.id,
      referencedTable: $db.contactDetails,
      getReferencedColumn: (t) => t.contactId,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$ContactDetailsTableFilterComposer(
            $db: $db,
            $table: $db.contactDetails,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return f(composer);
  }
}

class $$ContactsTableOrderingComposer
    extends Composer<_$AppDatabase, $ContactsTable> {
  $$ContactsTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<int> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name => $composableBuilder(
    column: $table.name,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get code => $composableBuilder(
    column: $table.code,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get lastPrintDate => $composableBuilder(
    column: $table.lastPrintDate,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get initials => $composableBuilder(
    column: $table.initials,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get profileImage => $composableBuilder(
    column: $table.profileImage,
    builder: (column) => ColumnOrderings(column),
  );
}

class $$ContactsTableAnnotationComposer
    extends Composer<_$AppDatabase, $ContactsTable> {
  $$ContactsTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<int> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<String> get name =>
      $composableBuilder(column: $table.name, builder: (column) => column);

  GeneratedColumn<String> get code =>
      $composableBuilder(column: $table.code, builder: (column) => column);

  GeneratedColumn<DateTime> get lastPrintDate => $composableBuilder(
    column: $table.lastPrintDate,
    builder: (column) => column,
  );

  GeneratedColumn<String> get initials =>
      $composableBuilder(column: $table.initials, builder: (column) => column);

  GeneratedColumn<String> get profileImage => $composableBuilder(
    column: $table.profileImage,
    builder: (column) => column,
  );

  Expression<T> contactDetailsRefs<T extends Object>(
    Expression<T> Function($$ContactDetailsTableAnnotationComposer a) f,
  ) {
    final $$ContactDetailsTableAnnotationComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.id,
      referencedTable: $db.contactDetails,
      getReferencedColumn: (t) => t.contactId,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$ContactDetailsTableAnnotationComposer(
            $db: $db,
            $table: $db.contactDetails,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return f(composer);
  }
}

class $$ContactsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $ContactsTable,
          Contact,
          $$ContactsTableFilterComposer,
          $$ContactsTableOrderingComposer,
          $$ContactsTableAnnotationComposer,
          $$ContactsTableCreateCompanionBuilder,
          $$ContactsTableUpdateCompanionBuilder,
          (Contact, $$ContactsTableReferences),
          Contact,
          PrefetchHooks Function({bool contactDetailsRefs})
        > {
  $$ContactsTableTableManager(_$AppDatabase db, $ContactsTable table)
    : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$ContactsTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$ContactsTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$ContactsTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<int> id = const Value.absent(),
                Value<String> name = const Value.absent(),
                Value<String> code = const Value.absent(),
                Value<DateTime?> lastPrintDate = const Value.absent(),
                Value<String> initials = const Value.absent(),
                Value<String?> profileImage = const Value.absent(),
              }) => ContactsCompanion(
                id: id,
                name: name,
                code: code,
                lastPrintDate: lastPrintDate,
                initials: initials,
                profileImage: profileImage,
              ),
          createCompanionCallback:
              ({
                Value<int> id = const Value.absent(),
                required String name,
                required String code,
                Value<DateTime?> lastPrintDate = const Value.absent(),
                required String initials,
                Value<String?> profileImage = const Value.absent(),
              }) => ContactsCompanion.insert(
                id: id,
                name: name,
                code: code,
                lastPrintDate: lastPrintDate,
                initials: initials,
                profileImage: profileImage,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable(table),
                  $$ContactsTableReferences(db, table, e),
                ),
              )
              .toList(),
          prefetchHooksCallback: ({contactDetailsRefs = false}) {
            return PrefetchHooks(
              db: db,
              explicitlyWatchedTables: [
                if (contactDetailsRefs) db.contactDetails,
              ],
              addJoins: null,
              getPrefetchedDataCallback: (items) async {
                return [
                  if (contactDetailsRefs)
                    await $_getPrefetchedData<
                      Contact,
                      $ContactsTable,
                      ContactDetail
                    >(
                      currentTable: table,
                      referencedTable: $$ContactsTableReferences
                          ._contactDetailsRefsTable(db),
                      managerFromTypedResult: (p0) => $$ContactsTableReferences(
                        db,
                        table,
                        p0,
                      ).contactDetailsRefs,
                      referencedItemsForCurrentItem: (item, referencedItems) =>
                          referencedItems.where((e) => e.contactId == item.id),
                      typedResults: items,
                    ),
                ];
              },
            );
          },
        ),
      );
}

typedef $$ContactsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $ContactsTable,
      Contact,
      $$ContactsTableFilterComposer,
      $$ContactsTableOrderingComposer,
      $$ContactsTableAnnotationComposer,
      $$ContactsTableCreateCompanionBuilder,
      $$ContactsTableUpdateCompanionBuilder,
      (Contact, $$ContactsTableReferences),
      Contact,
      PrefetchHooks Function({bool contactDetailsRefs})
    >;
typedef $$ContactDetailsTableCreateCompanionBuilder =
    ContactDetailsCompanion Function({
      Value<int> id,
      required int contactId,
      Value<bool> isPrinted,
      Value<DateTime?> lastPrint,
      required String name1,
      required String name2,
      required String name3,
      required ContactType type,
    });
typedef $$ContactDetailsTableUpdateCompanionBuilder =
    ContactDetailsCompanion Function({
      Value<int> id,
      Value<int> contactId,
      Value<bool> isPrinted,
      Value<DateTime?> lastPrint,
      Value<String> name1,
      Value<String> name2,
      Value<String> name3,
      Value<ContactType> type,
    });

final class $$ContactDetailsTableReferences
    extends BaseReferences<_$AppDatabase, $ContactDetailsTable, ContactDetail> {
  $$ContactDetailsTableReferences(
    super.$_db,
    super.$_table,
    super.$_typedResult,
  );

  static $ContactsTable _contactIdTable(_$AppDatabase db) =>
      db.contacts.createAlias(
        $_aliasNameGenerator(db.contactDetails.contactId, db.contacts.id),
      );

  $$ContactsTableProcessedTableManager get contactId {
    final $_column = $_itemColumn<int>('contact_id')!;

    final manager = $$ContactsTableTableManager(
      $_db,
      $_db.contacts,
    ).filter((f) => f.id.sqlEquals($_column));
    final item = $_typedResult.readTableOrNull(_contactIdTable($_db));
    if (item == null) return manager;
    return ProcessedTableManager(
      manager.$state.copyWith(prefetchedData: [item]),
    );
  }
}

class $$ContactDetailsTableFilterComposer
    extends Composer<_$AppDatabase, $ContactDetailsTable> {
  $$ContactDetailsTableFilterComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnFilters<int> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<bool> get isPrinted => $composableBuilder(
    column: $table.isPrinted,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<DateTime> get lastPrint => $composableBuilder(
    column: $table.lastPrint,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name1 => $composableBuilder(
    column: $table.name1,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name2 => $composableBuilder(
    column: $table.name2,
    builder: (column) => ColumnFilters(column),
  );

  ColumnFilters<String> get name3 => $composableBuilder(
    column: $table.name3,
    builder: (column) => ColumnFilters(column),
  );

  ColumnWithTypeConverterFilters<ContactType, ContactType, String> get type =>
      $composableBuilder(
        column: $table.type,
        builder: (column) => ColumnWithTypeConverterFilters(column),
      );

  $$ContactsTableFilterComposer get contactId {
    final $$ContactsTableFilterComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.contactId,
      referencedTable: $db.contacts,
      getReferencedColumn: (t) => t.id,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$ContactsTableFilterComposer(
            $db: $db,
            $table: $db.contacts,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return composer;
  }
}

class $$ContactDetailsTableOrderingComposer
    extends Composer<_$AppDatabase, $ContactDetailsTable> {
  $$ContactDetailsTableOrderingComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  ColumnOrderings<int> get id => $composableBuilder(
    column: $table.id,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<bool> get isPrinted => $composableBuilder(
    column: $table.isPrinted,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<DateTime> get lastPrint => $composableBuilder(
    column: $table.lastPrint,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name1 => $composableBuilder(
    column: $table.name1,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name2 => $composableBuilder(
    column: $table.name2,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get name3 => $composableBuilder(
    column: $table.name3,
    builder: (column) => ColumnOrderings(column),
  );

  ColumnOrderings<String> get type => $composableBuilder(
    column: $table.type,
    builder: (column) => ColumnOrderings(column),
  );

  $$ContactsTableOrderingComposer get contactId {
    final $$ContactsTableOrderingComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.contactId,
      referencedTable: $db.contacts,
      getReferencedColumn: (t) => t.id,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$ContactsTableOrderingComposer(
            $db: $db,
            $table: $db.contacts,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return composer;
  }
}

class $$ContactDetailsTableAnnotationComposer
    extends Composer<_$AppDatabase, $ContactDetailsTable> {
  $$ContactDetailsTableAnnotationComposer({
    required super.$db,
    required super.$table,
    super.joinBuilder,
    super.$addJoinBuilderToRootComposer,
    super.$removeJoinBuilderFromRootComposer,
  });
  GeneratedColumn<int> get id =>
      $composableBuilder(column: $table.id, builder: (column) => column);

  GeneratedColumn<bool> get isPrinted =>
      $composableBuilder(column: $table.isPrinted, builder: (column) => column);

  GeneratedColumn<DateTime> get lastPrint =>
      $composableBuilder(column: $table.lastPrint, builder: (column) => column);

  GeneratedColumn<String> get name1 =>
      $composableBuilder(column: $table.name1, builder: (column) => column);

  GeneratedColumn<String> get name2 =>
      $composableBuilder(column: $table.name2, builder: (column) => column);

  GeneratedColumn<String> get name3 =>
      $composableBuilder(column: $table.name3, builder: (column) => column);

  GeneratedColumnWithTypeConverter<ContactType, String> get type =>
      $composableBuilder(column: $table.type, builder: (column) => column);

  $$ContactsTableAnnotationComposer get contactId {
    final $$ContactsTableAnnotationComposer composer = $composerBuilder(
      composer: this,
      getCurrentColumn: (t) => t.contactId,
      referencedTable: $db.contacts,
      getReferencedColumn: (t) => t.id,
      builder:
          (
            joinBuilder, {
            $addJoinBuilderToRootComposer,
            $removeJoinBuilderFromRootComposer,
          }) => $$ContactsTableAnnotationComposer(
            $db: $db,
            $table: $db.contacts,
            $addJoinBuilderToRootComposer: $addJoinBuilderToRootComposer,
            joinBuilder: joinBuilder,
            $removeJoinBuilderFromRootComposer:
                $removeJoinBuilderFromRootComposer,
          ),
    );
    return composer;
  }
}

class $$ContactDetailsTableTableManager
    extends
        RootTableManager<
          _$AppDatabase,
          $ContactDetailsTable,
          ContactDetail,
          $$ContactDetailsTableFilterComposer,
          $$ContactDetailsTableOrderingComposer,
          $$ContactDetailsTableAnnotationComposer,
          $$ContactDetailsTableCreateCompanionBuilder,
          $$ContactDetailsTableUpdateCompanionBuilder,
          (ContactDetail, $$ContactDetailsTableReferences),
          ContactDetail,
          PrefetchHooks Function({bool contactId})
        > {
  $$ContactDetailsTableTableManager(
    _$AppDatabase db,
    $ContactDetailsTable table,
  ) : super(
        TableManagerState(
          db: db,
          table: table,
          createFilteringComposer: () =>
              $$ContactDetailsTableFilterComposer($db: db, $table: table),
          createOrderingComposer: () =>
              $$ContactDetailsTableOrderingComposer($db: db, $table: table),
          createComputedFieldComposer: () =>
              $$ContactDetailsTableAnnotationComposer($db: db, $table: table),
          updateCompanionCallback:
              ({
                Value<int> id = const Value.absent(),
                Value<int> contactId = const Value.absent(),
                Value<bool> isPrinted = const Value.absent(),
                Value<DateTime?> lastPrint = const Value.absent(),
                Value<String> name1 = const Value.absent(),
                Value<String> name2 = const Value.absent(),
                Value<String> name3 = const Value.absent(),
                Value<ContactType> type = const Value.absent(),
              }) => ContactDetailsCompanion(
                id: id,
                contactId: contactId,
                isPrinted: isPrinted,
                lastPrint: lastPrint,
                name1: name1,
                name2: name2,
                name3: name3,
                type: type,
              ),
          createCompanionCallback:
              ({
                Value<int> id = const Value.absent(),
                required int contactId,
                Value<bool> isPrinted = const Value.absent(),
                Value<DateTime?> lastPrint = const Value.absent(),
                required String name1,
                required String name2,
                required String name3,
                required ContactType type,
              }) => ContactDetailsCompanion.insert(
                id: id,
                contactId: contactId,
                isPrinted: isPrinted,
                lastPrint: lastPrint,
                name1: name1,
                name2: name2,
                name3: name3,
                type: type,
              ),
          withReferenceMapper: (p0) => p0
              .map(
                (e) => (
                  e.readTable(table),
                  $$ContactDetailsTableReferences(db, table, e),
                ),
              )
              .toList(),
          prefetchHooksCallback: ({contactId = false}) {
            return PrefetchHooks(
              db: db,
              explicitlyWatchedTables: [],
              addJoins:
                  <
                    T extends TableManagerState<
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic,
                      dynamic
                    >
                  >(state) {
                    if (contactId) {
                      state =
                          state.withJoin(
                                currentTable: table,
                                currentColumn: table.contactId,
                                referencedTable: $$ContactDetailsTableReferences
                                    ._contactIdTable(db),
                                referencedColumn:
                                    $$ContactDetailsTableReferences
                                        ._contactIdTable(db)
                                        .id,
                              )
                              as T;
                    }

                    return state;
                  },
              getPrefetchedDataCallback: (items) async {
                return [];
              },
            );
          },
        ),
      );
}

typedef $$ContactDetailsTableProcessedTableManager =
    ProcessedTableManager<
      _$AppDatabase,
      $ContactDetailsTable,
      ContactDetail,
      $$ContactDetailsTableFilterComposer,
      $$ContactDetailsTableOrderingComposer,
      $$ContactDetailsTableAnnotationComposer,
      $$ContactDetailsTableCreateCompanionBuilder,
      $$ContactDetailsTableUpdateCompanionBuilder,
      (ContactDetail, $$ContactDetailsTableReferences),
      ContactDetail,
      PrefetchHooks Function({bool contactId})
    >;

class $AppDatabaseManager {
  final _$AppDatabase _db;
  $AppDatabaseManager(this._db);
  $$ContactsTableTableManager get contacts =>
      $$ContactsTableTableManager(_db, _db.contacts);
  $$ContactDetailsTableTableManager get contactDetails =>
      $$ContactDetailsTableTableManager(_db, _db.contactDetails);
}
