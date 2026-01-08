// Manual parser for GridView widget
// Part of FlutterSharp Phase 4 - List Widgets

import 'dart:ffi';

import 'package:flutter/material.dart';
import '../flutter_sharp_structs.dart';
import '../generated/structs/gridview_struct.dart';
import '../utils.dart';
import '../maui_flutter.dart';

/// Parser for GridView widget.
///
/// A scrollable, 2D array of widgets.
///
/// GridView is a commonly used scrolling widget that displays its children
/// in a grid pattern. It scrolls in one direction and fills the cross axis.
///
/// This parser implements GridView.count functionality.
class GridViewWidgetParser extends WidgetParser {
  @override
  Widget? parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    final map = Pointer<GridViewStruct>.fromAddress(fos.handle.address).ref;

    // Get widget ID for debugging/tracking
    final id = parseString(map.id);
    if (id == null) return null;

    // Parse children
    List<Widget> children = [];
    if (map.children.address != 0) {
      children = DynamicWidgetBuilder.buildWidgets(
          map.children.cast<ChildrenStruct>(), buildContext);
    }

    // Parse crossAxisCount (default 2)
    int crossAxisCount = 2;
    if (map.hasCrossAxisCount == 1) {
      crossAxisCount = map.crossAxisCount;
    }

    // Parse spacing values (default 0.0)
    double mainAxisSpacing = 0.0;
    if (map.hasMainAxisSpacing == 1) {
      mainAxisSpacing = map.mainAxisSpacing;
    }

    double crossAxisSpacing = 0.0;
    if (map.hasCrossAxisSpacing == 1) {
      crossAxisSpacing = map.crossAxisSpacing;
    }

    double childAspectRatio = 1.0;
    if (map.hasChildAspectRatio == 1) {
      childAspectRatio = map.childAspectRatio;
    }

    // Parse scroll direction
    Axis scrollDirection = Axis.vertical;
    if (map.hasScrollDirection == 1) {
      scrollDirection = map.scrollDirection == 0 ? Axis.horizontal : Axis.vertical;
    }

    // Parse boolean properties
    final reverse = map.hasReverse == 1 ? map.reverse == 1 : false;
    final shrinkWrap = map.hasShrinkWrap == 1 ? map.shrinkWrap == 1 : false;
    final primary = map.hasPrimary == 1 ? map.primary == 1 : null;
    final addAutomaticKeepAlives = map.hasAddAutomaticKeepAlives == 1
        ? map.addAutomaticKeepAlives == 1
        : true;
    final addRepaintBoundaries = map.hasAddRepaintBoundaries == 1
        ? map.addRepaintBoundaries == 1
        : true;
    final addSemanticIndexes = map.hasAddSemanticIndexes == 1
        ? map.addSemanticIndexes == 1
        : true;

    // Parse optional cacheExtent
    final cacheExtent = map.hasCacheExtent == 1 ? map.cacheExtent : null;

    return GridView.count(
      crossAxisCount: crossAxisCount,
      mainAxisSpacing: mainAxisSpacing,
      crossAxisSpacing: crossAxisSpacing,
      childAspectRatio: childAspectRatio,
      scrollDirection: scrollDirection,
      reverse: reverse,
      shrinkWrap: shrinkWrap,
      primary: primary,
      cacheExtent: cacheExtent,
      addAutomaticKeepAlives: addAutomaticKeepAlives,
      addRepaintBoundaries: addRepaintBoundaries,
      addSemanticIndexes: addSemanticIndexes,
      children: children,
    );
  }

  @override
  String get widgetName => "GridView";
}

// The following classes are legacy code kept for reference.
// They are not used by the current parser implementation.

class GridViewWidget extends StatefulWidget {
  final GridViewParams _params;

  final BuildContext _buildContext;

  GridViewWidget(this._params, this._buildContext);

  @override
  _GridViewWidgetState createState() => _GridViewWidgetState(_params);
}

class _GridViewWidgetState extends State<GridViewWidget> {
  GridViewParams _params;
  List<Widget> _items = [];

  ScrollController _scrollController = new ScrollController();
  bool isPerformingRequest = false;

  //If there are no more items, it should not try to load more data while scroll
  //to bottom.
  bool loadCompleted = false;

  _GridViewWidgetState(this._params) {
    if (_params.children != null) {
      _items.addAll(_params.children);
    }
  }

  @override
  void initState() {
    super.initState();
    if (_params.loadMoreUrl == null || _params.loadMoreUrl.isEmpty) {
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
    return new SliverToBoxAdapter(
      child: Visibility(
        child: Padding(
          padding: const EdgeInsets.all(8.0),
          child: new Center(
            child: new Opacity(
              opacity: isPerformingRequest ? 1.0 : 0.0,
              child: new CircularProgressIndicator(),
            ),
          ),
        ),
        visible: !loadCompleted,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    var footer = _buildProgressIndicator();
    var sliverGrid = SliverPadding(
      padding: _params.padding,
      sliver: SliverGrid(
        gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: _params.crossAxisCount,
            mainAxisSpacing: _params.mainAxisSpacing,
            crossAxisSpacing: _params.crossAxisSpacing,
            childAspectRatio: _params.childAspectRatio),
        delegate: SliverChildBuilderDelegate(
          (BuildContext context, int index) {
            return _items[index];
          },
          childCount: _items.length,
        ),
      ),
    );

    return new CustomScrollView(
      slivers: <Widget>[sliverGrid, footer],
      controller: _scrollController,
      scrollDirection: _params.scrollDirection,
      reverse: _params.reverse,
      shrinkWrap: _params.shrinkWrap,
      cacheExtent: _params.cacheExtent,
    );
  }
}

class GridViewParams {
  int crossAxisCount;
  Axis scrollDirection;
  bool reverse;
  bool shrinkWrap;
  double cacheExtent;
  EdgeInsetsGeometry padding;
  double mainAxisSpacing;
  double crossAxisSpacing;
  double childAspectRatio;
  List<Widget> children;

  int pageSize;
  String loadMoreUrl;

  //use for demo, if true, it will do the fake request.
  bool isDemo;

  GridViewParams(
      this.crossAxisCount,
      this.scrollDirection,
      this.reverse,
      this.shrinkWrap,
      this.cacheExtent,
      this.padding,
      this.mainAxisSpacing,
      this.crossAxisSpacing,
      this.childAspectRatio,
      this.children,
      this.pageSize,
      this.loadMoreUrl,
      this.isDemo);
}
