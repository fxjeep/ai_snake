import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:drift/drift.dart' as drift;
import '../database/database.dart';
import '../core/theme.dart';

class ContactDialog extends StatefulWidget {
  final Contact? contact;

  const ContactDialog({super.key, this.contact});

  @override
  State<ContactDialog> createState() => _ContactDialogState();
}

class _ContactDialogState extends State<ContactDialog> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _nameController;
  late TextEditingController _codeController;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.contact?.name ?? '');
    _codeController = TextEditingController(text: widget.contact?.code ?? '');
  }

  @override
  void dispose() {
    _nameController.dispose();
    _codeController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final db = Provider.of<AppDatabase>(context, listen: false);
    final isEditing = widget.contact != null;

    return Theme(
      data: Theme.of(context).copyWith(
        dialogBackgroundColor: AppTheme.surfaceColor,
        textTheme: Theme.of(context).textTheme.apply(bodyColor: Colors.white, displayColor: Colors.white),
      ),
      child: AlertDialog(
        title: Text(isEditing ? 'Edit Contact' : 'Add Contact'),
        content: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: _nameController,
                decoration: const InputDecoration(
                  labelText: 'Name',
                  labelStyle: TextStyle(color: AppTheme.secondaryTextColor),
                  enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: AppTheme.secondaryTextColor)),
                ),
                validator: (value) => value == null || value.isEmpty ? 'Please enter a name' : null,
              ),
              const SizedBox(height: 20),
              TextFormField(
                controller: _codeController,
                decoration: const InputDecoration(
                  labelText: 'Code',
                  labelStyle: TextStyle(color: AppTheme.secondaryTextColor),
                  enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: AppTheme.secondaryTextColor)),
                ),
                validator: (value) => value == null || value.isEmpty ? 'Please enter a code' : null,
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel', style: TextStyle(color: AppTheme.secondaryTextColor)),
          ),
          ElevatedButton(
            onPressed: () async {
              if (_formKey.currentState!.validate()) {
                final initials = _getInitials(_nameController.text);
                if (isEditing) {
                  await db.updateContact(widget.contact!.copyWith(
                    name: _nameController.text,
                    code: _codeController.text,
                    initials: initials,
                  ));
                } else {
                  await db.addContact(ContactsCompanion.insert(
                    name: _nameController.text,
                    code: _codeController.text,
                    initials: initials,
                  ));
                }
                if (mounted) Navigator.pop(context);
              }
            },
            child: Text(isEditing ? 'Update' : 'Add'),
          ),
        ],
      ),
    );
  }

  String _getInitials(String name) {
    if (name.isEmpty) return '';
    final parts = name.split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0][0].toUpperCase();
  }
}
