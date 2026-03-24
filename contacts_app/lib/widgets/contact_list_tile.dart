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
    final isPrinted = contact.isPrinted;

    return InkWell(
      onTap: () => _handleAction(context, db, 'detail'),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 10.0, horizontal: 16.0),
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
            // Last Print column
            SizedBox(
              width: 100,
              child: Text(
                contact.lastPrintDate != null
                    ? contact.lastPrintDate!.toIso8601String().substring(0, 10)
                    : '',
                textAlign: TextAlign.center,
                style: const TextStyle(
                  color: AppTheme.secondaryTextColor,
                  fontSize: 13,
                ),
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
                const PopupMenuItem(value: 'merge_into', child: Text('Merge into')),
                const PopupMenuItem(value: 'delete', child: Text('Delete')),
              ],
              icon: const Icon(Icons.more_vert, color: AppTheme.secondaryTextColor),
            ),
          ],
        ),
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
        if (!contact.isPrinted) {
          await db.updateContact(contact.copyWith(
            isPrinted: true,
          ));
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Marked as printed')),
            );
          }
        } else {
          await db.updateContact(contact.copyWith(
            isPrinted: false,
            // We can keep lastPrintDate as the history of the last time it was printed
          ));
          if (context.mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(content: Text('Marked as unprinted')),
            );
          }
        }
        break;
      case 'merge_into':
        _showMergeDialog(context, db);
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

  void _showMergeDialog(BuildContext context, AppDatabase db) {
    Contact? selectedTarget;
    final TextEditingController textController = TextEditingController();
    final FocusNode focusNode = FocusNode();

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: AppTheme.surfaceColor,
        title: Text('Merge ${contact.name} / ${contact.code} into:'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            RawAutocomplete<Contact>(
              textEditingController: textController,
              focusNode: focusNode,
              displayStringForOption: (Contact option) => '${option.name} (${option.code})',
              optionsBuilder: (TextEditingValue textEditingValue) async {
                if (textEditingValue.text.isEmpty) {
                  return const Iterable<Contact>.empty();
                }
                final results = await db.searchContactsFuture(textEditingValue.text);
                // Exclude current contact from options
                return results.where((c) => c.id != contact.id);
              },
              onSelected: (Contact selection) {
                selectedTarget = selection;
              },
              fieldViewBuilder: (context, controller, focusNode, onFieldSubmitted) {
                return TextField(
                  controller: controller,
                  focusNode: focusNode,
                  autofocus: true,
                  style: const TextStyle(color: Colors.white),
                  decoration: const InputDecoration(
                    hintText: 'Search target contact (name or code)',
                    hintStyle: TextStyle(color: AppTheme.secondaryTextColor),
                    enabledBorder: UnderlineInputBorder(borderSide: BorderSide(color: Colors.white24)),
                    focusedBorder: UnderlineInputBorder(borderSide: BorderSide(color: AppTheme.primaryColor)),
                  ),
                  onSubmitted: (value) {
                    onFieldSubmitted();
                  },
                );
              },
              optionsViewBuilder: (context, onSelected, options) {
                return Align(
                  alignment: Alignment.topLeft,
                  child: Material(
                    elevation: 4.0,
                    color: const Color(0xFF1c2732),
                    borderRadius: BorderRadius.circular(8),
                    child: SizedBox(
                      width: 280, // Approximate width for the dialog content
                      child: ListView.builder(
                        padding: EdgeInsets.zero,
                        shrinkWrap: true,
                        itemCount: options.length,
                        itemBuilder: (BuildContext context, int index) {
                          final Contact option = options.elementAt(index);
                          return InkWell(
                            onTap: () => onSelected(option),
                            child: Padding(
                              padding: const EdgeInsets.all(16.0),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    option.name,
                                    style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                                  ),
                                  Text(
                                    option.code,
                                    style: const TextStyle(color: AppTheme.primaryColor, fontSize: 12),
                                  ),
                                ],
                              ),
                            ),
                          );
                        },
                      ),
                    ),
                  ),
                );
              },
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel', style: TextStyle(color: AppTheme.secondaryTextColor)),
          ),
          ElevatedButton(
            onPressed: () async {
              final query = textController.text.trim();
              if (query.isEmpty) return;

              // If they selected via autocomplete, we have selectedTarget.
              // If they just typed a name/code exactly, we should try to find it.
              Contact? target = selectedTarget;
              if (target == null) {
                target = await db.findContact(query);
              }

              if (target == null) {
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Target contact not found. Please select from dropdown.')),
                  );
                }
                return;
              }

              if (target.id == contact.id) {
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Cannot merge a contact into itself')),
                  );
                }
                return;
              }

              await db.mergeContacts(contact.id, target.id);
              if (context.mounted) {
                showDialog(
                  context: context,
                  barrierDismissible: false,
                  builder: (context) => AlertDialog(
                    backgroundColor: AppTheme.surfaceColor,
                    title: const Text('Merge completed', style: TextStyle(color: Colors.white)),
                    actions: [
                      TextButton(
                        onPressed: () {
                          Navigator.pop(context); // Close completion dialog
                          Navigator.pop(context); // Close merge dialog
                        },
                        child: const Text('OK'),
                      ),
                    ],
                  ),
                );
              }
            },
            child: const Text('Merge'),
          ),
        ],
      ),
    );
  }
}
