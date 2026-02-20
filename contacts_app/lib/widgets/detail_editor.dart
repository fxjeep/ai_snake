import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:drift/drift.dart' show Value;
import 'package:intl/intl.dart';
import '../core/theme.dart';
import '../database/database.dart';

class DetailEditor extends StatefulWidget {
  final Contact contact;

  const DetailEditor({super.key, required this.contact});

  @override
  State<DetailEditor> createState() => _DetailEditorState();
}

class _DetailEditorState extends State<DetailEditor> {
  ContactType _selectedType = ContactType.Live;
  bool _showValidationErrors = false;
  final TextEditingController _name1Controller = TextEditingController();
  final TextEditingController _name2Controller = TextEditingController();
  final TextEditingController _name3Controller = TextEditingController();
  final TextEditingController _searchController = TextEditingController();
  final FocusNode _name1FocusNode = FocusNode();

  @override
  void dispose() {
    _name1Controller.dispose();
    _name2Controller.dispose();
    _name3Controller.dispose();
    _searchController.dispose();
    _name1FocusNode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final db = Provider.of<AppDatabase>(context);

    return Scaffold(
      backgroundColor: AppTheme.backgroundColor,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 16),
              // Header
              IconButton(
                padding: EdgeInsets.zero,
                constraints: const BoxConstraints(),
                icon: const Icon(
                  Icons.arrow_back_ios_new,
                  color: AppTheme.primaryColor,
                  size: 24,
                ),
                onPressed: () => Navigator.pop(context),
              ),
              const SizedBox(height: 16),
              Text(
                widget.contact.name,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 48,
                  fontWeight: FontWeight.bold,
                ),
              ),
              Text(
                'ID: ${widget.contact.code}',
                style: const TextStyle(
                  color: AppTheme.primaryColor,
                  fontSize: 18,
                  fontWeight: FontWeight.w500,
                ),
              ),
              const SizedBox(height: 24),

              // Search and Action Buttons
              Row(
                children: [
                  Expanded(
                    child: Container(
                      height: 50,
                      decoration: BoxDecoration(
                        color: AppTheme.surfaceColor,
                        borderRadius: BorderRadius.circular(25),
                      ),
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      child: TextField(
                        controller: _searchController,
                        onChanged: (value) {
                          setState(() {}); // Trigger rebuild of StreamBuilder
                        },
                        style: const TextStyle(color: Colors.white),
                        decoration: const InputDecoration(
                          hintText: 'Search...',
                          hintStyle: TextStyle(color: AppTheme.secondaryTextColor),
                          prefixIcon: Icon(Icons.search, color: AppTheme.primaryColor),
                          border: InputBorder.none,
                          contentPadding: EdgeInsets.symmetric(vertical: 12),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              Row(
                children: [
                  _ActionButton(
                    icon: Icons.print,
                    label: 'Print',
                    color: AppTheme.primaryColor,
                    isSolid: true,
                  ),
                  const SizedBox(width: 12),
                  _ActionButton(
                    icon: Icons.delete,
                    label: 'Delete',
                    color: Colors.redAccent,
                    isSolid: false,
                    borderColor: Colors.redAccent.withOpacity(0.3),
                  ),
                  const SizedBox(width: 12),
                  // Type Dropdown
                  Expanded(
                    child: Container(
                      height: 48,
                      decoration: BoxDecoration(
                        color: AppTheme.surfaceColor,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: AppTheme.primaryColor.withOpacity(0.3)),
                      ),
                      padding: const EdgeInsets.symmetric(horizontal: 12),
                      child: DropdownButtonHideUnderline(
                        child: DropdownButton<ContactType>(
                          value: _selectedType,
                          dropdownColor: AppTheme.surfaceColor,
                          icon: const Icon(Icons.arrow_drop_down, color: AppTheme.primaryColor),
                          isExpanded: true,
                          items: ContactType.values.map((ContactType type) {
                            return DropdownMenuItem<ContactType>(
                              value: type,
                              child: Text(
                                type.name.toUpperCase(),
                                style: const TextStyle(
                                  color: Colors.white,
                                  fontSize: 14,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            );
                          }).toList(),
                          onChanged: (ContactType? newValue) {
                            if (newValue != null) {
                              setState(() {
                                _selectedType = newValue;
                                _showValidationErrors = false;
                                _name1Controller.clear();
                                _name2Controller.clear();
                                _name3Controller.clear();
                              });
                            }
                          },
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  _ActionButton(
                    icon: Icons.more_horiz,
                    label: 'more',
                    color: AppTheme.primaryColor,
                    isSolid: false,
                    backgroundColor: AppTheme.surfaceColor,
                    onPressed: () => _addDummyDetail(context, db),
                  ),
                ],
              ),
              const SizedBox(height: 24),

              // Dynamic Input Fields
              Row(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  ..._buildInputFields(),
                  const SizedBox(width: 12),
                  Container(
                    decoration: BoxDecoration(
                      color: AppTheme.primaryColor,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: IconButton(
                      icon: const Icon(Icons.add, color: Colors.white),
                      onPressed: () => _addNewDetail(context, db),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 24),

              // Table Header
              const Padding(
                padding: EdgeInsets.symmetric(horizontal: 8.0),
                child: Row(
                  children: [
                    const SizedBox(
                      width: 24,
                      height: 24,
                      child: _CustomCheckbox(value: false),
                    ),
                    const SizedBox(width: 8),
                    const SizedBox(width: 24), // Space for isPrint icon/checkbox
                    const SizedBox(width: 16),
                    Expanded(
                      child: Text(
                        'NAME 1',
                        style: TextStyle(
                          color: AppTheme.primaryColor,
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    Expanded(
                      child: Text(
                        'NAME 2',
                        style: TextStyle(
                          color: AppTheme.primaryColor,
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    Expanded(
                      child: Text(
                        'NAME 3',
                        style: TextStyle(
                          color: AppTheme.primaryColor,
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    Expanded(
                      child: Text(
                        'LAST PRINT',
                        style: TextStyle(
                          color: AppTheme.primaryColor,
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 8),
              const Divider(color: Colors.white10),

              // Table Rows
              Expanded(
                child: StreamBuilder<List<ContactDetail>>(
                  stream: db.watchDetailsByType(
                    widget.contact.id,
                    _selectedType,
                    query: _searchController.text.trim(),
                  ),
                  builder: (context, snapshot) {
                    if (!snapshot.hasData || snapshot.data!.isEmpty) {
                      return const Center(
                        child: Text(
                          'No Record',
                          style: TextStyle(color: AppTheme.secondaryTextColor),
                        ),
                      );
                    }
                    final details = snapshot.data!;

                    return ListView.builder(
                      itemCount: details.length,
                      itemBuilder: (context, index) {
                        final detail = details[index];
                        return _DetailRow(
                          isSelected: false, // For row selection
                          isPrinted: detail.isPrinted,
                          name1: detail.name1,
                          name2: detail.name2,
                          name3: detail.name3,
                          lastPrint: detail.lastPrint != null
                              ? DateFormat('MMM dd, yyyy').format(detail.lastPrint!)
                              : '-',
                        );
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  List<Widget> _buildInputFields() {
    final db = Provider.of<AppDatabase>(context, listen: false);
    switch (_selectedType) {
      case ContactType.Live:
        return [
          _buildTextField(_name1Controller, 'Name 1', (val) => _addNewDetail(context, db), 
              focusNode: _name1FocusNode, flex: 1),
        ];
      case ContactType.Dead:
        return [
          _buildTextField(_name1Controller, 'Name 1', (val) => _addNewDetail(context, db), 
              focusNode: _name1FocusNode, flex: 1),
          const SizedBox(width: 8),
          _buildTextField(_name2Controller, 'Name 2', (val) => _addNewDetail(context, db), flex: 1),
          const SizedBox(width: 8),
          _buildTextField(_name3Controller, 'Name 3', (val) => _addNewDetail(context, db), flex: 1),
        ];
      case ContactType.Ancestor:
        return [
          _buildTextField(_name1Controller, 'Name 1', (val) => _addNewDetail(context, db), 
              focusNode: _name1FocusNode, flex: 1),
          const SizedBox(width: 8),
          _buildTextField(_name2Controller, 'Name 2', (val) => _addNewDetail(context, db), flex: 1),
        ];
      case ContactType.Property:
        return [
          _buildTextField(_name1Controller, 'Property Details', (val) => _addNewDetail(context, db), 
              focusNode: _name1FocusNode, flex: 3),
        ];
    }
  }

  Widget _buildTextField(TextEditingController controller, String hint, Function(String)? onSubmitted,
      {FocusNode? focusNode, int flex = 1}) {
    final bool isEmpty = controller.text.trim().isEmpty;
    final bool showError = _showValidationErrors && isEmpty;

    return Expanded(
      flex: flex,
      child: TextField(
        controller: controller,
        focusNode: focusNode,
        style: const TextStyle(color: Colors.white),
        onSubmitted: onSubmitted,
        onChanged: (val) {
          if (_showValidationErrors) {
            setState(() {}); // Refresh to update borders
          }
        },
        decoration: InputDecoration(
          hintText: hint,
          hintStyle: const TextStyle(color: AppTheme.secondaryTextColor),
          isDense: true,
          contentPadding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
          filled: true,
          fillColor: AppTheme.surfaceColor,
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: BorderSide(
              color: showError ? Colors.redAccent : AppTheme.primaryColor.withOpacity(0.3),
              width: showError ? 2.0 : 1.0,
            ),
          ),
          enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: BorderSide(
              color: showError ? Colors.redAccent : AppTheme.primaryColor.withOpacity(0.1),
              width: showError ? 2.0 : 1.0,
            ),
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: BorderSide(
              color: showError ? Colors.redAccent : AppTheme.primaryColor,
              width: 2.0,
            ),
          ),
        ),
      ),
    );
  }

  void _addNewDetail(BuildContext context, AppDatabase db) async {
    bool hasEmptyVisibleFields = false;
    
    // Check visible fields based on selected type
    if (_name1Controller.text.trim().isEmpty) hasEmptyVisibleFields = true;
    
    if (_selectedType == ContactType.Dead) {
      if (_name2Controller.text.trim().isEmpty) hasEmptyVisibleFields = true;
      if (_name3Controller.text.trim().isEmpty) hasEmptyVisibleFields = true;
    } else if (_selectedType == ContactType.Ancestor) {
      if (_name2Controller.text.trim().isEmpty) hasEmptyVisibleFields = true;
    }

    if (hasEmptyVisibleFields) {
      setState(() {
        _showValidationErrors = true;
      });
      return;
    }

    final name1 = _name1Controller.text.trim();
    final name2 = _name2Controller.text.trim();
    final name3 = _name3Controller.text.trim();

    await db.addDetail(ContactDetailsCompanion.insert(
      contactId: widget.contact.id,
      name1: name1,
      name2: (_selectedType == ContactType.Dead || _selectedType == ContactType.Ancestor) ? name2 : '',
      name3: (_selectedType == ContactType.Dead) ? name3 : '',
      type: _selectedType,
      isPrinted: const Value(true),
      lastPrint: const Value(null),
    ));

    _name1Controller.clear();
    _name2Controller.clear();
    _name3Controller.clear();
    setState(() {
      _showValidationErrors = false;
    });

    _name1FocusNode.requestFocus();

    if (context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Added ${_selectedType.name} record')),
      );
    }
  }

  void _addDummyDetail(BuildContext context, AppDatabase db) async {
    await db.addDetail(ContactDetailsCompanion.insert(
      contactId: widget.contact.id,
      name1: 'New',
      name2: 'Detail',
      name3: 'Item',
      type: _selectedType,
    ));
    if (context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Added dummy ${_selectedType.name} detail')),
      );
    }
  }
}

class _ActionButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final bool isSolid;
  final Color? borderColor;
  final Color? backgroundColor;
  final VoidCallback? onPressed;

  const _ActionButton({
    required this.icon,
    required this.label,
    required this.color,
    required this.isSolid,
    this.borderColor,
    this.backgroundColor,
    this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: GestureDetector(
        onTap: onPressed,
        child: Container(
          height: 48,
          decoration: BoxDecoration(
            color: isSolid ? color : (backgroundColor ?? Colors.transparent),
            borderRadius: BorderRadius.circular(12),
            border: borderColor != null ? Border.all(color: borderColor!, width: 2) : null,
            boxShadow: isSolid
                ? [
                    BoxShadow(
                      color: color.withOpacity(0.4),
                      blurRadius: 10,
                      offset: const Offset(0, 4),
                    )
                  ]
                : null,
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(icon, color: isSolid ? Colors.white : color, size: 24),
              const SizedBox(width: 8),
              Text(
                label,
                style: TextStyle(
                  color: isSolid ? Colors.white : color,
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  final bool isSelected;
  final bool isPrinted;
  final String name1;
  final String name2;
  final String name3;
  final String lastPrint;

  const _DetailRow({
    required this.isSelected,
    required this.isPrinted,
    required this.name1,
    required this.name2,
    required this.name3,
    required this.lastPrint,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 8),
      decoration: const BoxDecoration(
        border: Border(bottom: BorderSide(color: Colors.white10)),
      ),
      child: Row(
        children: [
          SizedBox(
            width: 24,
            height: 24,
            child: _CustomCheckbox(value: isSelected),
          ),
          const SizedBox(width: 8),
          SizedBox(
            width: 24,
            height: 24,
            child: Icon(
              isPrinted ? Icons.print : Icons.print_disabled,
              size: 16,
              color: isPrinted ? AppTheme.primaryColor : AppTheme.secondaryTextColor,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Text(
              name1,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 16,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          Expanded(
            child: Text(
              name2,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 16,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          Expanded(
            child: Text(
              name3,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 16,
              ),
            ),
          ),
          Expanded(
            child: Text(
              lastPrint,
              style: const TextStyle(
                color: AppTheme.secondaryTextColor,
                fontSize: 14,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _CustomCheckbox extends StatelessWidget {
  final bool value;
  const _CustomCheckbox({required this.value});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: value ? AppTheme.primaryColor : Colors.transparent,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(
          color: value ? AppTheme.primaryColor : Colors.white24,
          width: 2,
        ),
      ),
      child: value
          ? const Icon(
              Icons.check,
              size: 16,
              color: Colors.white,
            )
          : null,
    );
  }
}
