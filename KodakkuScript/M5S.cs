using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommons;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Xml.Linq;
using Dalamud.Utility.Numerics;
using System.Collections;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;
using Microsoft.VisualBasic.Logging;
using System.Reflection;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;


namespace KodakkuScript
{
	[ScriptType(name: "至天の座アルカディア:零式 クルーザー級1", territorys: [1257], guid: "783C797E-52BB-41ED-98CD-A2315533036F", version: "0.0.0.1", note: noteStr, author: "UMP")]

	internal class M5S
	{
		const string noteStr =
	"""
        简易版，指路等攻略稳定再添加

        """;
		[UserSetting("启用Debug输出")]
		public bool EnableDev { get; set; }
		string debugOutput = "";
		double parse = 0;

		List<int> Dance = [0, 0, 0, 0, 0, 0, 0, 0];




		public void Init(ScriptAccessory accessory)
		{
			accessory.Method.RemoveDraw(".*");
			debugOutput = "";
			parse = 1.0;
			Dance = [0, 0, 0, 0, 0, 0, 0, 0];
		}

		[ScriptMethod(name: "钢铁月环_范围显示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4287[68])$"])]
		public void 钢铁月环_范围显示(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1.0) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			//42876-先钢铁，42878-先月环
			if (@event.ActionId == 42876) 
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "先钢铁";
				dp.Scale = new(7);
				dp.Owner = sid;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "后月环";
				dp.Scale = new(40);
				dp.InnerScale = new(5);
				dp.Radian = float.Pi * 2;
				dp.Owner = sid;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.Delay = 5000;
				dp.DestoryAt = 2500;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
			}
			if (@event.ActionId == 42878)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "先月环";
				dp.Scale = new(40);
				dp.InnerScale = new(5);
				dp.Radian = float.Pi * 2;
				dp.Owner = sid;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "后钢铁";
				dp.Scale = new(7);
				dp.Owner = sid;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.Delay = 5000;
				dp.DestoryAt = 2500;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
			}

		}

		[ScriptMethod(name: "跳舞_方向记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4276[2345])$"], userControl: false)]
		public void 跳舞_方向记录(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1.0) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			//小青蛙半场刀 4276X  4-打东 3-打北 5-打西 2-打南
			//Dance: 0-未知 1-南 2-东 3-北 4-西
			for (int i = 0; i < 8; i++)
			{
				if (Dance[i] == 0)
				{
					Dance[i] = @event.ActionId switch
					{
						42762 => 1,
						42764 => 2,
						42763 => 3,
						42765 => 4,
						_ => 0,
					};
					return;
				}
			}
		}

		[ScriptMethod(name: "跳舞1_范围显示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42858)$"])]
		public void 跳舞1_范围显示(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1.0) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			//结算 第一次 42858 5.8s  第二次 41872 5.8s +2.5每次
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞1";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[0] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 6000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞2";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[1] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞3";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[2] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 8500;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞4";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[3] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 11000;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞5";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[4] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 13500;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞6";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[5] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 16000;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞7";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[6] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 18500;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞8";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[7] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 21000;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
		}

		[ScriptMethod(name: "跳舞2_范围显示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41872)$"])]
		public void 跳舞2_范围显示(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1.0) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			//结算 第一次 42858 5.8s  第二次 41872 5.8s +2.5每次
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞1";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[0] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 6000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞2";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[1] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞3";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[2] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 7500;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞4";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[3] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 9000;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞5";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[4] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 10500;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞6";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[5] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 12000;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞7";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[6] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 13500;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "跳舞8";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[7] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 15000;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
		}

		[ScriptMethod(name: "跳舞_记录清理", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4286[34])$"], userControl: false)]
		public void 跳舞_记录清理(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1.0) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			Dance = [0, 0, 0, 0, 0, 0, 0, 0];
		}

		[ScriptMethod(name: "各种半场刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4278[89])|42869|42870)$"])]
		public void 场外分身半场刀(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1.0) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			if (@event.ActionId == 42788 || @event.ActionId == 42869)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "场外分身半场刀";
				dp.Scale = new(80, 80);
				dp.Owner = sid;
				dp.Rotation = float.Pi / -2;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
			}
			if (@event.ActionId == 42789 || @event.ActionId == 42870)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "场外分身半场刀";
				dp.Scale = new(80, 80);
				dp.Owner = sid;
				dp.Rotation = float.Pi / 2;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
			}

		}



		#region Utility
		private static bool ParseObjectId(string? idStr, out uint id)
		{
			id = 0;
			if (string.IsNullOrEmpty(idStr)) return false;
			try
			{
				var idStr2 = idStr.Replace("0x", "");
				id = uint.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
		{

			Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

			var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
			var lenth = v2.Length();
			return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
		}


		#endregion

	}
}
