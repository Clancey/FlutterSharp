import 'dart:ffi';

import 'package:ffi/ffi.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import 'package:flutter_module/utils.dart';
import '../maui_flutter.dart';

class TextFieldParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<TextFieldStruct>.fromAddress(fos.handle.address).ref;
    final id = parseString(map.id);
    return SimpleTextField(
      id,
      text: parseString(map.value),
      decoration: InputDecoration(
          border: OutlineInputBorder(), hintText: parseString(map.hint)),
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
        raiseMauiEvent(id, "onInput", value);
      },
      decoration: widget.decoration,
    );
  }
}
