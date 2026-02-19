import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:drift/drift.dart' hide Column;
import '../database/database.dart';
import '../core/theme.dart';
import 'contact_dialog.dart';
import 'detail_editor.dart';

class ContactListTile extends StatelessWidget {
  final Contact contact;

  const ContactListTile({super.key, required this.contact});

  @override
  Widget build(BuildContext context) {
    final db = Provider.of<AppDatabase>(context, listen: false);
    final isPrinted = contact.lastPrintDate != null;

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 10.0),
      child: Row(
        children: [
          if (isPrinted) ...[
            const Icon(
              Icons.print,
              size: 16,
              color: AppTheme.primaryColor,
            ),
            const SizedBox(width: 8),
          ],
          // Info
          Expanded(
            child: Row(
              children: [
                Text(
                  contact.name,
                  style: const TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.w600,
                    color: Colors.white,
                  ),
                ),
                const SizedBox(width: 8),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                  decoration: BoxDecoration(
                    color: const Color(0xFF1c2732),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: Text(
                    contact.code,
                    style: const TextStyle(
                      color: AppTheme.primaryColor,
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),
          ),
          // Actions
          PopupMenuButton<String>(
            onSelected: (value) => _handleAction(context, db, value),
            itemBuilder: (context) => [
              const PopupMenuItem(value: 'detail', child: Text('Detail')),
              const PopupMenuItem(value: 'create_list', child: Text('Create List')),
              const PopupMenuItem(value: 'edit', child: Text('Edit')),
              PopupMenuItem(value: 'print', child: Text(isPrinted ? 'Unprint' : 'Print')),
              const PopupMenuItem(value: 'delete', child: Text('Delete')),
            ],
            icon: const Icon(Icons.more_vert, color: AppTheme.secondaryTextColor),
          ),
        ],
      ),
    );
  }



  void _handleAction(BuildContext context, AppDatabase db, String action) async {
    switch (action) {
      case 'detail':
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => DetailEditor(contact: contact),
          ),
        );
        break;
      case 'create_list':
        // Placeholder for now
        break;
      case 'edit':
        showDialog(
          context: context,
          builder: (context) => ContactDialog(contact: contact),
        );
        break;
      case 'print':
        if (contact.lastPrintDate == null) {
          await db.updateContact(contact.copyWith(lastPrintDate: Value(DateTime.now())));
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Marked as printed')),
            );
          }
        } else {
          await db.updateContact(contact.copyWith(lastPrintDate: const Value(null)));
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Marked as unprinted')),
            );
          }
        }
        break;
      case 'delete':
        final confirm = await showDialog<bool>(
          context: context,
          builder: (context) => Theme(
            data: Theme.of(context).copyWith(
              dialogBackgroundColor: AppTheme.surfaceColor,
            ),
            child: AlertDialog(
              title: const Text('Delete Contact'),
              content: Text('Are you sure you want to delete ${contact.name}?'),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(context, false),
                  child: const Text('Cancel', style: TextStyle(color: AppTheme.secondaryTextColor)),
                ),
                TextButton(
                  onPressed: () => Navigator.pop(context, true),
                  child: const Text('Confirm', style: TextStyle(color: Colors.redAccent)),
                ),
              ],
            ),
          ),
        );
        if (confirm == true) {
          await db.deleteContact(contact);
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Contact deleted')),
            );
          }
        }
        break;
    }
  }
}
