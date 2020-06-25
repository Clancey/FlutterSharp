import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import '../maui_flutter.dart';

class TextFieldParser extends WidgetParser {
  @override
  Widget parse(Map<String, dynamic> map, BuildContext buildContext) {
    final id = map["id"];
    return SimpleTextField(id,
      text: map['text'],
      decoration:
          InputDecoration(border: OutlineInputBorder(), hintText: map["hint"]),
    );
  }

  @override
  String get widgetName => "TextField";
}

class SimpleTextField extends StatefulWidget {
  SimpleTextField(this.id,
      {this.text,
      this.onInputEventHandlerId,
      this.onSubmitEventHandlerId,
      this.decoration}) {}
  String id;
  String text;
  int onInputEventHandlerId;
  int onSubmitEventHandlerId;
  InputDecoration decoration;

  @override
  _SimpleTextFieldState createState() => _SimpleTextFieldState(id);
}

class _SimpleTextFieldState extends State<SimpleTextField> {
  TextEditingController controller;

  _SimpleTextFieldState(this.id);
  String id;
  @override
  void initState() {
    super.initState();
    controller = TextEditingController(text: widget.text);
  }

  @override
  void dispose() {
    controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (widget.text != controller.text) {
      controller.text = widget.text ?? '';
      FocusScope.of(context).requestFocus(new FocusNode());
    }
    return TextField(
      controller: controller,
      onChanged: (value) {
        raiseMauiEvent(id, "onChange", value);
      },
      onSubmitted: (value) {
          raiseMauiEvent(id,"onInput",value);
      },
      decoration: widget.decoration,
    );
  }
}
