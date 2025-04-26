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
	[ScriptType(name: "至天の座アルカディア零式クルーザー級2", territorys: [1259], guid: "77D98242-9AF6-49B1-BA80-11AA83D40B11", version: "0.0.0.1", note: noteStr, author: "UMP")]

	internal class M6S
	{
		const string noteStr =
	"""
        Game8打法

        """;
		[UserSetting("启用Debug输出")]
		public bool EnableDev { get; set; }
		string debugOutput = "";

		public void Init(ScriptAccessory accessory)
		{
			accessory.Method.RemoveDraw(".*");
			debugOutput = "";
		}


		[ScriptMethod(name: "钢铁月环_范围显示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4287[68])$"])]
		public void 钢铁月环_范围显示(Event @event, ScriptAccessory accessory)
		{
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