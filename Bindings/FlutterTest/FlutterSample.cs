using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Internal;
using Flutter.Structs;

namespace FlutterTest {
	public class ListPage : StatefulWidget {
		List<string> items = new List<string> ();
		string itemText;
		public override Widget Build () =>
			new Column (MainAxisAlignment.Start) {
				new Row (MainAxisAlignment.Start) {
					new Flexible {
						new Container(padding: new EdgeInsetsGeometry(8)) {
							new TextField(hint:"What's to be done?") {
								OnInput = (s) => {
									SetState (() => {
										items.Add(s);
									});
								},
								OnChange = (s) => {
									itemText = s;
								}
							}
						}
					},
					new Container(padding: new EdgeInsetsGeometry (left: 8)) {
						new FloatingActionButton(onPressed: ()=>{
							if(!string.IsNullOrWhiteSpace(itemText))
								SetState(()=>{
									items.Add(itemText);
									itemText = null;
								});
						}){
							new Icon (
								codePoint: "58824",
								fontFamily : "MaterialIcons"),
						}
					}
				},
				new Flexible {
					new ListViewBuilder(
						count: items.Count,
						itemBuilder: (row) =>  new Text(items[(int)row])
					),
				}
			};
	}
	public class ClickedPage : StatefulWidget {
		int clicked = 0;
		public override Widget Build () =>
			new Center {
				new Column(MainAxisAlignment.SpaceAround) {
					new Text($"You have pressed {clicked} times"),
					new FloatingActionButton (onPressed:() => {
							SetState (() => {
								//Communicator.SendCommand(("intptr",handle.ToString()));
								clicked++;
							});
						}){
						new Icon(codePoint: "57669",fontFamily:"MaterialIcons")
					}
				}
			};
	}

	public class FlutterSample : StatelessWidget {
		public override Widget Build () => new DefaultTabController (2) {
				new Scaffold (appbar:new AppBar (
						title:new Text("Hello from Maui!"),
						bottom: new TabBar {
							new Tab{new Text("Counter")},
							new Tab{new Text("Todo List")}
						}),
					drawer: new Drawer {
						new Text("Add Navigation items!")
					},
					body: new TabBarView {
						new ClickedPage(),
						new ListPage(),
					}
			)
		};
	}
}
