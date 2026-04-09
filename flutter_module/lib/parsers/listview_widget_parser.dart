// Manual parser for ListView widget
// Part of FlutterSharp Phase 4 - List Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import '../scroll_controller_manager.dart';

/// Parser for ListView widget.
///
/// A scrollable list of widgets arranged linearly.
///
/// ListView is the most commonly used scrolling widget. It displays its
/// children one after another in the scroll direction.
class ListViewWidgetParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<ListViewStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for debugging/tracking
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse children
    List<Widget> children = [];
    if (map.children.address != 0) {
      children = DynamicWidgetBuilder.buildWidgets(
          map.children.cast<ChildrenStruct>(), buildContext);
    }

    // Parse scroll direction
    Axis scrollDirection = Axis.vertical;
    if (map.hasScrollDirection == 1) {
      scrollDirection =
          map.scrollDirection == 0 ? Axis.horizontal : Axis.vertical;
    }

    // Parse boolean properties
    final reverse = map.hasReverse == 1 ? map.reverse == 1 : false;
    final shrinkWrap = map.hasShrinkWrap == 1 ? map.shrinkWrap == 1 : false;
    final primary = map.hasPrimary == 1 ? map.primary == 1 : null;
    final addAutomaticKeepAlives = map.hasAddAutomaticKeepAlives == 1
        ? map.addAutomaticKeepAlives == 1
        : true;
    final addRepaintBoundaries =
        map.hasAddRepaintBoundaries == 1 ? map.addRepaintBoundaries == 1 : true;
    final addSemanticIndexes =
        map.hasAddSemanticIndexes == 1 ? map.addSemanticIndexes == 1 : true;

    // Parse optional double properties
    final itemExtent = map.hasItemExtent == 1 ? map.itemExtent : null;
    final cacheExtent = map.hasCacheExtent == 1 ? map.cacheExtent : null;

    // Parse controller ID and get/create the ScrollController
    ScrollController? controller;
    if (map.hasController == 1) {
      final controllerId = parseString(map.controllerId);
      if (controllerId != null && controllerId.isNotEmpty) {
        controller = scrollControllerManager.getController(controllerId);
      }
    }

    return ListView(
      scrollDirection: scrollDirection,
      reverse: reverse,
      shrinkWrap: shrinkWrap,
      primary: primary,
      controller: controller,
      itemExtent: itemExtent,
      cacheExtent: cacheExtent,
      addAutomaticKeepAlives: addAutomaticKeepAlives,
      addRepaintBoundaries: addRepaintBoundaries,
      addSemanticIndexes: addSemanticIndexes,
      children: children,
    );
  }

  @override
  String get widgetName => "ListView";
}

// The ListViewWidget and ListViewParams classes below are legacy code
// that was used for a more complex ListView implementation with load-more
// functionality. They are kept for reference but not used by the parser.

class ListViewWidget extends StatefulWidget {
  final ListViewParams _params;

  ListViewWidget(this._params, BuildContext _);

  @override
  _ListViewWidgetState createState() => _ListViewWidgetState(_params);
}

class _ListViewWidgetState extends State<ListViewWidget> {
  ListViewParams _params;
  List<Widget> _items = [];

  ScrollController _scrollController = new ScrollController();
  bool isPerformingRequest = false;

  //If there are no more items, it should not try to load more data while scroll
  //to bottom.
  bool loadCompleted = false;

  _ListViewWidgetState(this._params) {
    _items.addAll(_params.children);
  }

  @override
  void initState() {
    super.initState();
    if (_params.loadMoreUrl.isEmpty) {
      loadCompleted = true;
      return;
    }
    _scrollController.addListener(() {
      if (!loadCompleted &&
          _scrollController.position.pixels ==
              _scrollController.position.maxScrollExtent) {
        _getMoreData();
      }
    });
  }

  _getMoreData() async {
    if (!isPerformingRequest) {
      // Reserved for future load-more functionality
    }
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  Widget _buildProgressIndicator() {
    return new Padding(
      padding: const EdgeInsets.all(8.0),
      child: new Center(
        child: new Opacity(
          opacity: isPerformingRequest ? 1.0 : 0.0,
          child: new CircularProgressIndicator(),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      scrollDirection: _params.scrollDirection,
      reverse: _params.reverse,
      shrinkWrap: _params.shrinkWrap,
      cacheExtent: _params.cacheExtent,
      padding: _params.padding,
      itemCount: loadCompleted ? _items.length : _items.length + 1,
      itemBuilder: (context, index) {
        if (index == _items.length) {
          return _buildProgressIndicator();
        } else {
          return _items[index];
        }
      },
      controller: _scrollController,
    );
  }
}

class ListViewParams {
  Axis scrollDirection;
  bool reverse;
  bool shrinkWrap;
  double cacheExtent;
  EdgeInsetsGeometry? padding;
  double? itemExtent;
  List<Widget> children;

  int pageSize;
  String loadMoreUrl;

  //use for demo, if true, it will do the fake request.
  bool isDemo;

  ListViewParams(
      this.scrollDirection,
      this.reverse,
      this.shrinkWrap,
      this.cacheExtent,
      this.padding,
      this.itemExtent,
      this.children,
      this.pageSize,
      this.loadMoreUrl,
      this.isDemo);
}
