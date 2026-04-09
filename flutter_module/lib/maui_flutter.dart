library maui_flutter;

import 'dart:ffi';
import 'package:flutter/material.dart';
import 'package:flutter_module/flutter_sharp_structs.dart';
import 'package:flutter_module/utils.dart';
import 'mauiRenderer.dart';
export 'mauiRenderer.dart'
    show methodChannel, sendException, sendExceptionToCSharp;
export 'performance_overlay.dart'
    show
        FlutterSharpPerformanceOverlay,
        PerformanceOverlayManager,
        PerformanceOverlayToggle,
        OverlayPosition,
        performanceOverlayManager;
export 'rendering_metrics.dart'
    show
        RenderingMetricsManager,
        RenderingStats,
        FrameTimingInfo,
        renderingMetrics;
export 'binary_protocol.dart'
    show BinaryProtocol, BinaryProtocolStats, MessageTypes;
export 'widget_inspector_service.dart'
    show
        WidgetInspectorManager,
        WidgetInspectorService,
        WidgetInspectorOverlay,
        WidgetInspectorToggle,
        widgetInspectorManager,
        widgetInspectorService;
import 'dart:convert';

// Manual parser imports (legacy - kept for backwards compatibility)
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
import 'parsers/elevatedbutton_parser.dart';
import 'parsers/textbutton_parser.dart';
import 'parsers/outlinedbutton_parser.dart';
import 'parsers/iconbutton_parser.dart';
import 'parsers/floatingactionbutton_parser.dart';
import 'parsers/checkbox_parser.dart';
import 'parsers/radio_parser.dart';
import 'parsers/switch_parser.dart';
import 'parsers/slider_parser.dart';
import 'parsers/card_parser.dart';
import 'parsers/bottomnavigationbar_parser.dart';
import 'parsers/gridview_widget_parser.dart';
import 'parsers/icon_widget_parser.dart';
import 'parsers/image_widget_parser.dart';
import 'parsers/indexedstack_widget_parser.dart';
import 'parsers/listtile_widget_parser.dart';
import 'parsers/listview_widget_parser.dart';
import 'parsers/listviewbuilder_parser.dart';
import 'parsers/listenablebuilder_parser.dart';
import 'parsers/gridviewbuilder_parser.dart';
import 'parsers/provider_parser.dart';
import 'parsers/changenotifierprovider_parser.dart';
import 'parsers/consumer_parser.dart';
import 'parsers/selector_parser.dart';
import 'parsers/multiprovider_parser.dart';
import 'parsers/opacity_widget_parser.dart';
import 'parsers/padding_widget_parser.dart';
import 'parsers/pageview_widget_parser.dart';
import 'parsers/placeholder_widget_parser.dart';
import 'parsers/safearea_widget_parser.dart';
import 'parsers/scaffold_parser.dart';
import 'parsers/selectabletext_widget_parser.dart';
import 'parsers/singlechildscrollview_parser.dart';
import 'parsers/sizedbox_widget_parser.dart';
import 'parsers/stack_positioned_widgets_parser.dart';
import 'parsers/statefullwidget_parser.dart';
import 'parsers/tab_parser.dart';
import 'parsers/tabbar_parser.dart';
import 'parsers/tabbarview_parser.dart';
import 'parsers/text_widget_parser.dart';
import 'parsers/textfield_parser.dart';
import 'parsers/wrap_widget_parser.dart';
import 'parsers/navigator_parser.dart';
import 'parsers/materialapp_parser.dart';
import 'parsers/refreshindicator_parser.dart';
import 'parsers/infinitelistview_parser.dart';
import 'parsers/infinitegridview_parser.dart';
import 'parsers/alert_dialog_parser.dart';
import 'parsers/bottom_sheet_parser.dart';
import 'parsers/cupertinobutton_parser.dart';
import 'parsers/cupertinotextfield_parser.dart';
import 'parsers/cupertinonavigationbar_parser.dart';
import 'parsers/cupertinotabbar_parser.dart';
import 'parsers/cupertinoswitch_parser.dart';
import 'parsers/dropdownbutton_parser.dart';
import 'parsers/errorboundary_parser.dart';

import 'package:logging/logging.dart';

// Auto-generated parser imports and list (418 parsers from Flutter SDK)
import 'generated_parsers.dart';

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
    ContainerWidgetParser(),
    DefaultTabControllerParser(),
    DrawerParser(),
    DropCapTextParser(),
    ErrorBoundaryParser(),
    ExpandedSizedBoxWidgetParser(),
    ExpandedWidgetParser(),
    FittedBoxWidgetParser(),
    FlexibleParser(),
    ElevatedButtonParser(),
    TextButtonParser(),
    OutlinedButtonParser(),
    IconButtonParser(),
    FloatingActionButtonParser(),
    CheckboxParser(),
    RadioParser(),
    SwitchParser(),
    SliderParser(),
    CardParser(),
    BottomNavigationBarParser(),
    DropdownButtonParser(),
    GridViewWidgetParser(),
    IconWidgetParser(),
    IndexedStackWidgetParser(),
    ListTileWidgetParser(),
    ListViewWidgetParser(),
    ListViewBuilderParser(),
    ListenableBuilderParser(),
    GridViewBuilderParser(),
    InfiniteListViewParser(),
    InfiniteGridViewParser(),
    ProviderParser(),
    ChangeNotifierProviderParser(),
    ConsumerParser(),
    SelectorParser(),
    MultiProviderParser(),
    NetworkImageWidgetParser(),
    OpacityWidgetParser(),
    PaddingWidgetParser(),
    PageViewWidgetParser(),
    PlaceholderWidgetParser(),
    PositionedWidgetParser(),
    RaisedButtonParser(),
    SafeAreaWidgetParser(),
    ScaffoldParser(),
    SelectableTextWidgetParser(),
    SingleChildScrollViewParser(),
    // SizedBoxWidgetParser() - removed: legacy parser returned null, generated SizedBoxParser is used instead
    StackWidgetParser(),
    StatefulWidgetParser(),
    TabParser(),
    TabBarParser(),
    TabBarViewParser(),
    TextFieldParser(),
    TextWidgetParser(),
    WrapWidgetParser(),
    NavigatorParser(),
    MaterialAppParser(),
    RefreshIndicatorParser(),
    CupertinoButtonParser(),
    CupertinoTextFieldParser(),
    CupertinoNavigationBarParser(),
    CupertinoTabBarParser(),
    CupertinoSwitchParser(),
    AlertDialogParser(),
    BottomSheetParser(),

    // Add all generated parsers (418 parsers from Flutter SDK)
    ...generatedParsers,
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
        _widgetNameParserMap.putIfAbsent(parser.widgetName, () => parser);
      }
      _defaultParserInited = true;
    }
  }

  static Widget? buildFromAddress(int pointer, BuildContext buildContext) {
    if (pointer == 0) return null;
    var map = Pointer<WidgetStruct>.fromAddress(pointer);
    var widget = buildFromPointer(map, buildContext);
    return widget;
  }

  static final _trackedDartObjects = <String, dynamic>{};

  static Widget? buildFromPointer(
      Pointer<WidgetStruct> p, BuildContext buildContext) {
    if (p.address == 0) return null;
    return buildFromMap(p.ref, buildContext);
  }

  static Widget buildFromPointerNotNull(
      Pointer<WidgetStruct> p, BuildContext buildContext) {
    if (p.address == 0) return new Text("Null");
    return buildFromMap(p.ref, buildContext) ?? new Text("Null");
  }

  static Widget? buildFromMap(
      IFlutterObjectStruct? fos, BuildContext buildContext) {
    if (fos == null) return null;
    initDefaultParsersIfNess();
    String? widgetName = parseString(fos.widgetType);
    if (widgetName == null) return null;
    var parser = _widgetNameParserMap[widgetName];
    if (parser != null) {
      try {
        var w = parser.parse(fos, buildContext);
        return w;
      } catch (e, stackTrace) {
        log.severe("Parser error for $widgetName: $e");
        // Send exception to C# for logging
        sendException(
          e,
          stackTrace,
          errorType: 'ParserError',
          widgetType: widgetName,
          source: 'DynamicWidgetBuilder.buildFromMap',
          handledLocally: true,
        );
        return Text("Error parsing $widgetName: $e",
            style: const TextStyle(color: Colors.red));
      }
    }
    log.warning("Not support type: $widgetName");
    return Text("Unknown widget type $widgetName");
  }

  // TODO: Legacy method - ISingleChildRenderObjectWidgetStruct no longer exists
  // static Widget? buildMauiComponenet(
  //     ISingleChildRenderObjectWidgetStruct? map, BuildContext buildContext) {
  //   if (map == null) return null;
  //   String? id = parseString(map.id);
  //   if (id == null) return null;
  //   print("Creating MauiComponent :$id");
  //   var mc = new MauiComponent(componentId: id);
  //   print("Setting State MauiComponent :$id");
  //   if (map.child.address != 0) setMauiState(id, map.child.ref);
  //   print("Setting State Set :$id");
  //   return mc;
  // }

  void releaseTrackedDartObject(String id) {
    if (_trackedDartObjects.containsKey(id)) {
      _trackedDartObjects.remove(id);
    }
  }

  static List<Widget> buildWidgets(
      Pointer<ChildrenStruct> chldPtr, BuildContext buildContext) {
    List<Widget> rt = [];
    if (chldPtr.address != 0) {
      var childrenStruct = chldPtr.ref;
      if (childrenStruct.childrenLength > 0) {
        final values =
            childrenStruct.children.asTypedList(childrenStruct.childrenLength);
        for (var v in values) {
          var widget = buildFromAddress(v, buildContext);
          if (widget != null) rt.add(widget);
        }
      }
    }
    return rt;
  }
}

Future raiseMauiEvent(
    String componentId, String eventName, dynamic args) async {
  if (eventName == 'invoke' && componentId.startsWith('action_')) {
    final Map<String, dynamic>? actionArgs;
    if (args == null) {
      actionArgs = null;
    } else if (args is Map<String, dynamic>) {
      actionArgs = args;
    } else if (args is Map) {
      actionArgs = args.map((key, value) => MapEntry(key.toString(), value));
    } else {
      actionArgs = {'value': args};
    }

    return invokeHandleAction(componentId, args: actionArgs);
  }

  return methodChannel.invokeMethod(
      "Event",
      json.encode(
          {'eventName': eventName, 'componentId': componentId, 'data': args}));
}

Future<dynamic> requestMauiData(
    String componentId, String eventName, dynamic args) {
  return methodChannel.invokeMethod(
      "Event",
      json.encode({
        'eventName': eventName,
        'componentId': componentId,
        'needsReturn': true,
        'data': args
      }));
}

/// extends this class to make a Flutter widget parser.
abstract class WidgetParser {
  /// parse the json map into a flutter widget.
  Widget? parse(IFlutterObjectStruct map, BuildContext buildContext);

  /// the widget type name for example:
  /// {"type" : "Text", "data" : "Denny"}
  /// if you want to make a flutter Text widget, you should implement this
  /// method return "Text", for more details, please see
  /// @TextWidgetParser
  String get widgetName;
}
