library maui_flutter;

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'mauiRenderer.dart';
import 'dart:convert';

import 'parsers/align_widget_parser.dart';
import 'parsers/appbar_parser.dart';
import 'parsers/aspectratio_widget_parser.dart';
import 'parsers/baseline_widget_parser.dart';
import 'parsers/raisedbutton_widget_parser.dart';
import 'parsers/center_widget_parser.dart';
import 'parsers/cliprrect_widget_parser.dart';
import 'parsers/container_widget_parser.dart';
import 'parsers/defaulttabcontroller_parser.dart';
import 'parsers/drawer_parser.dart';
import 'parsers/dropcaptext_widget_parser.dart';
import 'parsers/expanded_widget_parser.dart';
import 'parsers/fittedbox_widget_parser.dart';
import 'parsers/flexible_parser.dart';
import 'parsers/floatingactionbutton_parser.dart';
import 'parsers/gridview_widget_parser.dart';
import 'parsers/icon_widget_parser.dart';
import 'parsers/image_widget_parser.dart';
import 'parsers/indexedstack_widget_parser.dart';
import 'parsers/listtile_widget_parser.dart';
import 'parsers/listview_widget_parser.dart';
import 'parsers/listviewbuilder_parser.dart';
import 'parsers/opacity_widget_parser.dart';
import 'parsers/padding_widget_parser.dart';
import 'parsers/pageview_widget_parser.dart';
import 'parsers/placeholder_widget_parser.dart';
import 'parsers/row_column_widget_parser.dart';
import 'parsers/safearea_widget_parser.dart';
import 'parsers/scaffold_parser.dart';
import 'parsers/selectabletext_widget_parser.dart';
import 'parsers/sizedbox_widget_parser.dart';
import 'parsers/stack_positioned_widgets_parser.dart';
import 'parsers/statefullwidget_parser.dart';
import 'parsers/tab_parser.dart';
import 'parsers/tabbar_parser.dart';
import 'parsers/tabbarview_parser.dart';
import 'parsers/text_widget_parser.dart';
import 'parsers/textfield_parser.dart';
import 'parsers/wrap_widget_parser.dart';

import 'package:flutter/widgets.dart';
import 'package:logging/logging.dart';

// import 'dynamic_widget/basic/cliprrect_widget_parser.dart';

class DynamicWidgetBuilder {
  static final Logger log = Logger('DynamicWidget');

  static final _parsers = [
    AlignWidgetParser(),
    AppBarParser(),
    AspectRatioWidgetParser(),
    AssetImageWidgetParser(),
    BaselineWidgetParser(),
    CenterWidgetParser(),
    ClipRRectWidgetParser(),
    ColumnWidgetParser(),
    ContainerWidgetParser(),
    DefaultTabControllerParser(),
    DrawerParser(),
    DropCapTextParser(),
    ExpandedSizedBoxWidgetParser(),
    ExpandedWidgetParser(),
    FittedBoxWidgetParser(),
    FlexibleParser(),
    FloatingActionButtonParser(),
    GridViewWidgetParser(),
    IconWidgetParser(),
    IndexedStackWidgetParser(),
    ListTileWidgetParser(),
    ListViewWidgetParser(),
    ListViewBuilderParser(),
    NetworkImageWidgetParser(),
    OpacityWidgetParser(),
    PaddingWidgetParser(),
    PageViewWidgetParser(),
    PlaceholderWidgetParser(),
    PositionedWidgetParser(),
    RaisedButtonParser(),
    RowWidgetParser(),
    SafeAreaWidgetParser(),
    ScaffoldParser(),
    SelectableTextWidgetParser(),
    SizedBoxWidgetParser(),
    StackWidgetParser(),
    StatefulWidgetParser(),
    TabParser(),
    TabBarParser(),
    TabBarViewParser(),
    TextFieldParser(),
    TextWidgetParser(),
    WrapWidgetParser(),
  ];

  static final _widgetNameParserMap = <String, WidgetParser>{};

  static bool _defaultParserInited = false;

  // use this method for adding your custom widget parser
  static void addParser(WidgetParser parser) {
    log.info(
        "add custom widget parser, make sure you don't overwirte the widget type.");
    _parsers.add(parser);
    _widgetNameParserMap[parser.widgetName] = parser;
  }

  static void initDefaultParsersIfNess() {
    if (!_defaultParserInited) {
      for (var parser in _parsers) {
        _widgetNameParserMap[parser.widgetName] = parser;
      }
      _defaultParserInited = true;
    }
  }

  static Widget buildFromJson(String json, BuildContext buildContext) {
    if(json == null)
      return null;
    var map = jsonDecode(json);
    var widget = buildFromMap(map, buildContext);
    return widget;
  }

  static final _trackedDartObjects = <String, dynamic>{};

  static Widget buildFromMap(
      Map<String, dynamic> map, BuildContext buildContext) {
    if (map == null)
      return null;
    String widgetName = map['type'];
    initDefaultParsersIfNess();
    //TODO: Bring back ID
    var parser = _widgetNameParserMap[widgetName];
    if (parser != null) {
      //if(!wrapInComponent)
        return parser.parse(map, buildContext);

    }
    log.warning("Not support type: $widgetName");
    return Text("Unknown widget type $widgetName");
  }


    static Widget buildMauiComponenet(Map<String, dynamic> map, BuildContext buildContext)
    {
      if (map == null)
        return null;
      String id = map['id'];
      var mc = new MauiComponent(componentId: id);
      setMauiState(id,map['child']);
      return mc;
    }

  void releaseTrackedDartObject(String id) {
    if (_trackedDartObjects.containsKey(id)) {
      _trackedDartObjects.remove(id);
    }
  }

  static List<Widget> buildWidgets(
      List<dynamic> values, BuildContext buildContext) {
    List<Widget> rt = [];
    if(values != null)
    for (var value in values) {
      rt.add(buildFromMap(value, buildContext));
    }
    return rt;
  }
}
Future raiseMauiEvent(String componentId, String eventName, dynamic args) async {
  return methodChannel.invokeMethod("Event",json.encode({
    'eventName': eventName,
    'componentId': componentId,
    'data': args
  }));
}

Future<dynamic> requestMauiData(String componentId, String eventName, dynamic args) {
  return methodChannel.invokeMethod("Event",json.encode({
    'eventName': eventName,
    'componentId': componentId,
    'needsReturn': true,
    'data': args
  }));
}

/// extends this class to make a Flutter widget parser.
abstract class WidgetParser {
  /// parse the json map into a flutter widget.
  Widget parse(Map<String, dynamic> map, BuildContext buildContext);

  /// the widget type name for example:
  /// {"type" : "Text", "data" : "Denny"}
  /// if you want to make a flutter Text widget, you should implement this
  /// method return "Text", for more details, please see
  /// @TextWidgetParser
  String get widgetName;
}
