import 'dart:async';
import 'dart:convert';
import 'dart:ffi';
import '../flutter_sharp_structs.dart';
import '../utils.dart';
import '../maui_flutter.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
// import 'package:http/http.dart' as http;

class ListViewWidgetParser extends WidgetParser {
  @override
  Widget parse(IFlutterObjectStruct fos, BuildContext buildContext) {
    var map = Pointer<MultiChildRenderObjectWidgetStruct>.fromAddress(fos.handle.address).ref;
    return null;
    //TODO: Implement;
    // var scrollDirection = Axis.vertical;
    // if (map.containsKey("scrollDirection") &&
    //     "horizontal" == map["scrollDirection"]) {
    //   scrollDirection = Axis.horizontal;
    // }

    // var reverse = map.containsKey("reverse") ? map['reverse'] : false;
    // var shrinkWrap = map.containsKey("shrinkWrap") ? map["shrinkWrap"] : false;
    // var cacheExtent = map.containsKey("cacheExtent") ? map["cacheExtent"] : 0.0;
    // var padding = map.containsKey('padding')
    //     ? parseEdgeInsetsGeometry(map['padding'])
    //     : null;
    // var itemExtent = map.containsKey("itemExtent") ? map["itemExtent"] : null;
    // var children = DynamicWidgetBuilder.buildWidgets(
    //     map['children'], buildContext);
    // var pageSize = map.containsKey("pageSize") ? map["pageSize"] : 10;
    // var loadMoreUrl =
    //     map.containsKey("loadMoreUrl") ? map["loadMoreUrl"] : null;
    // var isDemo = map.containsKey("isDemo") ? map["isDemo"] : false;

    // var params = new ListViewParams(
    //     scrollDirection,
    //     reverse,
    //     shrinkWrap,
    //     cacheExtent,
    //     padding,
    //     itemExtent,
    //     children,
    //     pageSize,
    //     loadMoreUrl,
    //     isDemo);

    // return new ListViewWidget(params, buildContext);
  }

  @override
  String get widgetName => "ListView";
}

class ListViewWidget extends StatefulWidget {
  final ListViewParams _params;
  final BuildContext _buildContext;

  ListViewWidget(this._params, this._buildContext);

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
      // setState(() => isPerformingRequest = true);
      // //TODO: Make a request to the app
      // var jsonString = "";//_params.isDemo ? await fakeRequest() : await doRequest();
      // var buildWidgets = DynamicWidgetBuilder.buildWidgets(
      //     jsonDecode(jsonString), widget._buildContext);
      // setState(() {
      //   if (buildWidgets.isEmpty) {
      //     loadCompleted = true;
      //   }
      //   _items.addAll(buildWidgets);
      //   isPerformingRequest = false;
      // });
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
  EdgeInsetsGeometry padding;
  double itemExtent;
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
