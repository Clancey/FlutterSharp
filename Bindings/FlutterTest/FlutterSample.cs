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
							new TextField {
								Hint = "What's to be done?",
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
							new Icon {
								CodePoint = "58824",
								FontFamily = "MaterialIcons"
							},
						}
					}
				},
				new Flexible {
					new ListViewBuilder {
						ItemCount = items.Count,
						ItemBuilder = (row) =>  new Text(items[(int)row]),
					}
				}
			};
	}
	public class ClickedPage : StatefulWidget {
		int clicked = 0;
		public override Widget Build () =>
			new Center {
				new Column(MainAxisAlignment.SpaceAround) {
					new Text($"You have pressed {clicked} times"),
					new FloatingActionButton {
						OnPressed = () => {
							SetState (() => {
								//Communicator.SendCommand(("intptr",handle.ToString()));
								clicked++;
							});
						},
						Child = new Icon{ CodePoint="57669",FontFamily="MaterialIcons"},}}};
	}

	public class FlutterSample : StatelessWidget {
		public override Widget Build () => new DefaultTabController (2) {
				new Scaffold {
					AppBar = new AppBar {
						Title = new Text("Hello from Maui!"),
						Bottom = new TabBar {
							new Tab{new Text("Counter")},
							new Tab{new Text("Todo List")},}},
					Drawer = new Drawer {
						new Text("Add Navigation items!")
					},
					Body = new TabBarView {
						new ClickedPage(),
						new ListPage(),
						}}};
	}
}
