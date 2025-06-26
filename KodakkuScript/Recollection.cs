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
using KodakkuAssist.Module.GameOperate;
using System.Reflection;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using ECommons.DalamudServices;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using KodakkuAssist.Module.Draw.Manager;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Dalamud.Plugin.Services;
using ECommons.MathHelpers;

namespace KodakkuScript
{
	[ScriptType(name: "極ゼレニア討滅戦", territorys: [1271], guid: "6192A434-05E0-4E7E-9724-1CC855E9C975", version: "0.0.0.6", note: noteStr, author: "UMP")]

	public class Recollection
	{
		const string noteStr =
	"""
        Game8打法，开箱即用

        """;
		[UserSetting("启用Debug输出")]
		public bool EnableDev { get; set; }

		string debugOutput = "";

		int parse = -1;

		List<int> P1Tower = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P2Tether = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P3_3Mark = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P3_3Index = [];
		List<int> P3Circle = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P3_4Mark = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P3_6Mark = [0, 0, 0, 0, 0, 0, 0, 0];

		bool THFirst = false;
		bool Map = false;
		bool Map2 = false;
		bool Map4 = false;
		bool Map6 = false;
		int P3_5Safe = 0;
		float P3_3North = float.Pi;

		//近刀 = 0， 远刀 = 1
		List<bool> 远近刀记录 = [];

		Vector3 CloseBase = new(100f, 0f, 94f);
		Vector3 FarBase = new(100f, 0f, 91f);
		Vector3 centre = new(100f, 0f, 100f);

		Vector3 东上内 = new(107.5f, 0f, 99.5f);
		Vector3 东下内 = new(107.5f, 0f, 100.5f);
		Vector3 东上外 = new(108.5f, 0f, 99.5f);
		Vector3 东下外 = new(108.5f, 0f, 100.5f);

		Vector3 西上内 = new(92.5f, 0f, 99.5f);
		Vector3 西下内 = new(92.5f, 0f, 100.5f);
		Vector3 西上外 = new(91.5f, 0f, 99.5f);
		Vector3 西下外 = new(91.5f, 0f, 100.5f);

		Vector3 南左内 = new(99.5f, 0f, 107.5f);
		//Vector3 南右内 = new(100.5f, 0f, 107.5f);
		Vector3 南左外 = new(99.5f, 0f, 108.5f);
		//Vector3 南右外 = new(100.5f, 0f, 108.5f);

		//Vector3 北左内 = new(99.5f, 0f, 92.5f);
		Vector3 北右内 = new(100.5f, 0f, 92.5f);
		//Vector3 北左外 = new(99.5f, 0f, 91.5f);
		Vector3 北右外 = new(100.5f, 0f, 91.5f);

		float CloseMutli67_5 = 4.15746f;
		float CloseMutli22_5 = 1.72208f;
		float FarMutli67_5 = 6.46716f;
		float FarMutli22_5 = 2.67878f;


		public void Init(ScriptAccessory accessory)
		{	
			accessory.Method.RemoveDraw(".*");
			debugOutput = "";
			parse = 1;
			P1Tower = [0, 0, 0, 0, 0, 0, 0, 0];
			P2Tether = [0, 0, 0, 0, 0, 0, 0, 0];
			P3_3Mark = [0, 0, 0, 0, 0, 0, 0, 0];
			P3_3Index = [];
			P3Circle = [0, 0, 0, 0, 0, 0, 0, 0];
			P3_4Mark = [0, 0, 0, 0, 0, 0, 0, 0];
			P3_6Mark = [0, 0, 0, 0, 0, 0, 0, 0];

			THFirst = false;
			Map = false;
			Map2 = false;
			Map4 = false;
			Map6 = false;
			P3_5Safe = 0;
			远近刀记录 = [];
			P3_3North = float.Pi;
		}

		[ScriptMethod(name: "开场_月环踩塔_点名记录", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0244"], userControl: false)]
		public void 开场_月环踩塔_点名记录(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(((uint)tid));
			P1Tower[tIndex] = 1;
		}

		[ScriptMethod(name: "开场_月环踩塔_指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43226"])]
		public void 开场_月环踩塔_指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P1Tower[myIndex] != 1) return;

			Vector3 TowerNW = new Vector3(92, 0, 95);
			Vector3 TowerNE = new Vector3(108, 0, 95);
			Vector3 TowerSE = new Vector3(108, 0, 105);
			Vector3 TowerSW = new Vector3(92, 0, 105);

			if (myIndex == 0 || myIndex == 6)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "开场_月环踩塔_指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = TowerNW;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 7000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1 || myIndex == 7)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "开场_月环踩塔_指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = TowerNE;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 7000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3 || myIndex == 5)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "开场_月环踩塔_指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = TowerSE;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 7000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2 || myIndex == 4)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "开场_月环踩塔_指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = TowerSW;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 7000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "第一轮远近刀_远近记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"], userControl: false)]
		public void 第一轮远近刀_远近记录(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			远近刀记录.Add(@event.StatusParam == 759);
			if (EnableDev)
			{
				debugOutput = @event.StatusParam == 759 ? "远刀" : "近刀";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
		}

		[ScriptMethod(name: "第一轮远近刀_指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"])]
		public async void 第一轮远近刀_指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//13-3-3-3
			//从正西开始顺时针1-8
			List<Vector3> ClosePos = [];
			List<Vector3> FarPos = [];

			ClosePos.Add(new Vector3(100 - CloseMutli22_5, 0, 100 - CloseMutli67_5));
			ClosePos.Add(new Vector3(100 + CloseMutli22_5, 0, 100 - CloseMutli67_5));
			ClosePos.Add(new Vector3(100 - CloseMutli67_5, 0, 100 - CloseMutli22_5));
			ClosePos.Add(new Vector3(100 + CloseMutli67_5, 0, 100 - CloseMutli22_5));
			ClosePos.Add(new Vector3(100 - CloseMutli22_5, 0, 100 + CloseMutli67_5));
			ClosePos.Add(new Vector3(100 + CloseMutli22_5, 0, 100 + CloseMutli67_5));
			ClosePos.Add(new Vector3(100 - CloseMutli67_5, 0, 100 + CloseMutli22_5));
			ClosePos.Add(new Vector3(100 + CloseMutli67_5, 0, 100 + CloseMutli22_5));

			FarPos.Add(new Vector3(100 - FarMutli22_5, 0, 100 - FarMutli67_5));
			FarPos.Add(new Vector3(100 + FarMutli22_5, 0, 100 - FarMutli67_5));
			FarPos.Add(new Vector3(100 - FarMutli67_5, 0, 100 - FarMutli22_5));
			FarPos.Add(new Vector3(100 + FarMutli67_5, 0, 100 - FarMutli22_5));
			FarPos.Add(new Vector3(100 - FarMutli22_5, 0, 100 + FarMutli67_5));
			FarPos.Add(new Vector3(100 + FarMutli22_5, 0, 100 + FarMutli67_5));
			FarPos.Add(new Vector3(100 - FarMutli67_5, 0, 100 + FarMutli22_5));
			FarPos.Add(new Vector3(100 + FarMutli67_5, 0, 100 + FarMutli22_5));

			await Task.Delay(1000);


			if (myIndex < 4)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第一轮_远近刀_指路1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				await Task.Delay(12000);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第一轮_远近刀_指路2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = 远近刀记录[0] != 远近刀记录[1] ? ClosePos[myIndex] : FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第一轮_远近刀_指路3";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 3000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第一轮_远近刀_指路4";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = 远近刀记录[0] != 远近刀记录[1] ? FarPos[myIndex] : ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex > 3)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第一轮_远近刀_指路1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				await Task.Delay(12000); 
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第一轮_远近刀_指路2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = 远近刀记录[0] != 远近刀记录[1] ? FarPos[myIndex] : ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第一轮_远近刀_指路3";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 3000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第一轮_远近刀_指路4";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = 远近刀记录[0] != 远近刀记录[1] ? ClosePos[myIndex] : FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}

		}

		/*[ScriptMethod(name: "第一轮远近刀_绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"], userControl: true)]
		public async void 第一轮远近刀_绘制(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1.0) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			Vector3 center = new(100, 0, 100);

			//13-3-3-3
			await Task.Delay(5000);

			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制1A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 7000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制1B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 7000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制1C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 7000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制1D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 7000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

			await Task.Delay(7000);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制3A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制3B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制3C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制3D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制4A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制4B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制4C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第一轮_远近刀_绘制4D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
		}*/

		[ScriptMethod(name: "圣护壁阶段_换P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43189"], userControl: false)]
		public void 圣护壁阶段_换P(Event @event, ScriptAccessory accessory)
		{
			parse = 2;
			远近刀记录 = [];
		}

		[ScriptMethod(name: "圣护壁_线收集", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0011"], userControl: false)]
		public void 圣护壁_线收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			var tIndex = accessory.Data.PartyList.IndexOf((uint)@event.TargetId);
			P2Tether[tIndex] = 1;
		}

		[ScriptMethod(name: "圣护壁_线指路", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0011"])]
		public async void 圣护壁_线指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			if (tid != accessory.Data.Me) return;
			else if (EnableDev)
			{
				debugOutput = "你被点了";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

			await Task.Delay(1500);

			var c = accessory.Data.Objects.SearchById(sid);
			if (c == null) return;
			else if (EnableDev)
			{
				debugOutput = "已获取到连线来源";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}

			var transformationID = ((KodakkuAssist.Data.IBattleChara)c).GetTransformationID();

			if (EnableDev)
			{
				debugOutput = c.Position.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");			
				debugOutput = transformationID.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}"""); 
			}
			Vector3 TH_N_Pos = new(pos.X, 0, pos.Z - 3);
			Vector3 TH_S_Pos = new(pos.X, 0, pos.Z + 3);
			Vector3 DPS_N_Pos = new(pos.X, 0, pos.Z - 3);
			Vector3 DPS_S_Pos = new(pos.X, 0, pos.Z + 3);
			//27-右刀，28-左刀
			if (myIndex < 4)
			{
				//TH组
				if (pos.X > 105f) return;
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "圣护壁_线指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = transformationID == 27 ? TH_S_Pos : TH_N_Pos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex > 3)
			{
				//DPS组
				if (pos.X < 95f) return;
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "圣护壁_线指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = transformationID == 27 ? DPS_N_Pos : DPS_S_Pos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "圣护壁_线清理", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43187"], userControl: false)]
		public void 圣护壁_线清理(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			P2Tether = [0, 0, 0, 0, 0, 0, 0, 0];
		}

		[ScriptMethod(name: "圣护壁_塔指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43068"])]
		public void 圣护壁_塔指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P2Tether[myIndex] == 1) return;
			var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
			if (myIndex < 4)
			{
				//TH组
				if (pos.X > 100) return;				
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "圣护壁_塔指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = pos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex > 3)
			{
				//DPS组
				if (pos.X < 100) return;
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "圣护壁_塔指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = pos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "后半阶段_换P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43213"], userControl: false)]
		public void 后半阶段_换P(Event @event, ScriptAccessory accessory)
		{
			parse = 3;
		}

		[ScriptMethod(name: "魔法阵展开_换P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43193"], userControl: false)]
		public void 魔法阵展开_换P(Event @event, ScriptAccessory accessory)
		{
			if (parse == 3) parse = 4;
			if (parse == 9) parse = 10;
		}

		[ScriptMethod(name: "魔法阵展开_地板收集", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void 魔法阵展开_地板收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4 && parse != 10) return;
			if (!int.TryParse(@event["Index"], out var index))return;
			if (index == 6)
			{
				Map = true;
			}
			if (index == 4)
			{
				Map = false;
			}
			if (EnableDev)
			{
				debugOutput = index.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			//04-右下起点顺时针-false，06-正上起点逆时针-true
		}

		[ScriptMethod(name: "魔法阵展开_指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43198"])]
		public void 魔法阵展开_指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4 && parse != 10) return;
			Vector3 NorthStart = new(99.2f, 0, 94.8f);
			Vector3 NorthEnd = new(90f, 0, 105.5f);
			Vector3 SouthStart = new(105.6f, 0, 102f);
			Vector3 SouthEnd = new(96.4f, 0, 105.5f);
			if (Map)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_指路1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NorthStart;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_指路2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NorthEnd;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			else
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_指路1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = SouthStart;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_指路2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = SouthEnd;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "魔法阵展开_二式_换P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43540"], userControl: false)]
		public void 魔法阵展开_二式_换P(Event @event, ScriptAccessory accessory)
		{
			parse = 5;
		}

		[ScriptMethod(name: "魔法阵展开_二式_地板收集", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void 魔法阵展开_二式_地板收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 5) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (index == 5)
			{
				Map2 = true;
			}
			if (index == 10)
			{
				Map2 = false;
			}
			if (EnableDev && (index == 5 || index == 10))
			{
				debugOutput = Map2 ? "钢左月右" : "钢右月左";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			//获取：右偏上内 红（05）/ 左偏上内 红（0A）
			//10-钢右月左-false，5-钢左月右-true
		}

		[ScriptMethod(name: "魔法阵展开_二式_指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4344[89])$"])]
		public void 魔法阵展开_二式_指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 5) return;

			//8-先钢铁，9-先月环
			if (@event.ActionId == 43448)
			{
				if (Map2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 西下外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 西上内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 西下内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 东下外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 东下内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 东上内;
					dp.Color = accessory.Data.DefaultSafeColor;					
					dp.Delay = 8000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (@event.ActionId == 43449)
			{
				if (Map2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 东下内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 东下外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 东上外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 8000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 西下内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 西上外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_二式_指路3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 西下外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

		[ScriptMethod(name: "魔法阵展开_三式_换P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43541"], userControl: false)]
		public void 魔法阵展开_三式_换P(Event @event, ScriptAccessory accessory)
		{
			parse = 6;
		}

		[ScriptMethod(name: "魔法阵展开_三式_地板收集", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void 魔法阵展开_三式_地板收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (index < 12)
			{
				P3_3Index.Add(index);
			}
			//获取：内圈红（正北顺 04-0B）4-11
		}

		[ScriptMethod(name: "魔法阵展开_三式_地板计算", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43195"], userControl: false)]
		public void 魔法阵展开_三式_地板计算(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (P3_3Index[0] * P3_3Index[1] == 0)
			{
				return;
			}
			if (P3_3Index[0] * P3_3Index[1] == 40)
			{
				P3_3North = 15 * float.Pi / 8;
			}
			else if(P3_3Index[0] * P3_3Index[1] == 55)
			{
				P3_3North = float.Pi / 8;
			}
			else
			{
				P3_3Index[0] -= 3;
				P3_3Index[1] -= 3;
				P3_3North = (P3_3Index[0] + P3_3Index[1]- 1) * float.Pi / 8;
			}
			if (EnableDev)
			{
				if (P3_3Index[0] * P3_3Index[1] == 40)
				{
					debugOutput = "8号地板为北";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
				else if (P3_3Index[0] * P3_3Index[1] == 55)
				{
					debugOutput = "1号地板为北";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
				else
				{
					float debugOutputnum = (P3_3Index[0] + P3_3Index[1]) / 2;
					debugOutput = $"""{debugOutputnum}号地板为北""";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}

				debugOutput = P3_3North.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			//特殊：1+7  2+8    (2n-1)pi/8
			//获取：内圈红（正北顺 04-0B）4-11
		}

		[ScriptMethod(name: "魔法阵展开_三式_点名收集", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"], userControl: false)]
		public void 魔法阵展开_三式_点名收集(Event @event, ScriptAccessory accessory)
		{			
			if (parse != 6) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(tid);
			P3_3Mark[tIndex] = 1;
		}

		[ScriptMethod(name: "魔法阵展开_三式_点名指路", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"])]
		public void 魔法阵展开_三式_点名指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//if (P3_3Mark[myIndex] != 1) return;
			if (tid != accessory.Data.Me) return;

			if (EnableDev)
			{
				debugOutput = "你要放花";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			if (myIndex == 0 || myIndex == 6)
			{
				var dealpos = RotatePoint(CloseBase, centre, P3_3North);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_三式_点名指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1 || myIndex == 5)
			{
				var dealpos = RotatePoint(CloseBase, centre, P3_3North + float.Pi);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_三式_点名指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2 || myIndex == 4)
			{
				var dealpos = RotatePoint(CloseBase, centre, P3_3North + (float.Pi * 5 / 4));

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_三式_点名指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3 || myIndex == 7)
			{
				var dealpos = RotatePoint(CloseBase, centre, P3_3North + (float.Pi * 3 / 4));

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_三式_点名指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "魔法阵展开_三式_塔指路", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"])]
		public async void 魔法阵展开_三式_塔指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			await Task.Delay(1000);
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P3_3Mark[myIndex] == 1) return;
			if (myIndex == 0 || myIndex == 6)
			{
				var dealpos = RotatePoint(FarBase, centre, P3_3North);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_三式_塔指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1 || myIndex == 5)
			{
				var dealpos = RotatePoint(FarBase, centre, P3_3North + float.Pi);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_三式_塔指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2 || myIndex == 4)
			{
				var dealpos = RotatePoint(FarBase, centre, P3_3North + (float.Pi * 3 / 2));

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_三式_塔指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3 || myIndex == 7)
			{
				var dealpos = RotatePoint(FarBase, centre, P3_3North + (float.Pi / 2));

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_三式_塔指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}

		}

		[ScriptMethod(name: "第二轮远近刀_月环记录", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0244"], userControl: false)]
		public void 第二轮远近刀_月环记录(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			if (tid != accessory.Data.Me) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (myIndex > 3)
			{
				THFirst = true;
			}
		}

		[ScriptMethod(name: "第二轮远近刀_远近记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"], userControl: false)]
		public void 第二轮远近刀_远近记录(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			远近刀记录.Add(@event.StatusParam == 759);
			if (EnableDev)
			{
				debugOutput = @event.StatusParam == 759 ? "远刀" : "近刀";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
		}

		[ScriptMethod(name: "第二轮远近刀_指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"])]
		public async void 第二轮远近刀_指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//13-3-3-3
			//从正西开始顺时针1-8
			List<Vector3> ClosePos = [];
			List<Vector3> FarPos = [];

			ClosePos.Add(new Vector3(100 - CloseMutli22_5, 0, 100 - CloseMutli67_5));
			ClosePos.Add(new Vector3(100 + CloseMutli22_5, 0, 100 - CloseMutli67_5));
			ClosePos.Add(new Vector3(100 - CloseMutli67_5, 0, 100 - CloseMutli22_5));
			ClosePos.Add(new Vector3(100 + CloseMutli67_5, 0, 100 - CloseMutli22_5));
			ClosePos.Add(new Vector3(100 - CloseMutli22_5, 0, 100 + CloseMutli67_5));
			ClosePos.Add(new Vector3(100 + CloseMutli22_5, 0, 100 + CloseMutli67_5));
			ClosePos.Add(new Vector3(100 - CloseMutli67_5, 0, 100 + CloseMutli22_5));
			ClosePos.Add(new Vector3(100 + CloseMutli67_5, 0, 100 + CloseMutli22_5));

			FarPos.Add(new Vector3(100 - FarMutli22_5, 0, 100 - FarMutli67_5));
			FarPos.Add(new Vector3(100 + FarMutli22_5, 0, 100 - FarMutli67_5));
			FarPos.Add(new Vector3(100 - FarMutli67_5, 0, 100 - FarMutli22_5));
			FarPos.Add(new Vector3(100 + FarMutli67_5, 0, 100 - FarMutli22_5));
			FarPos.Add(new Vector3(100 - FarMutli22_5, 0, 100 + FarMutli67_5));
			FarPos.Add(new Vector3(100 + FarMutli22_5, 0, 100 + FarMutli67_5));
			FarPos.Add(new Vector3(100 - FarMutli67_5, 0, 100 + FarMutli22_5));
			FarPos.Add(new Vector3(100 + FarMutli67_5, 0, 100 + FarMutli22_5));

			Vector3 WaitN = new Vector3(100, 0, 94.5f);
			Vector3 WaitS = new Vector3(100, 0, 105.5f);
			await Task.Delay(1000);

			if (远近刀记录.Count == 0)
			{
				debugOutput = "出现错误，请至DC反馈";
				accessory.Method.SendChat($"""/e {debugOutput}""");
				return;
			}

			if (THFirst)
			{
				if (myIndex < 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[0] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 12000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					await Task.Delay(12000);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[1] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[2] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 3000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[3] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myIndex > 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WaitS;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 12000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					await Task.Delay(12000);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[1] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[2] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 3000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[3] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}

			}
			else
			{
				if (myIndex < 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WaitN;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 12000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					await Task.Delay(12000);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[1] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[2] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 3000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[3] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myIndex > 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[0] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 12000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					await Task.Delay(12000);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[1] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[2] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 3000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "第二轮_远近刀_指路4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 远近刀记录[3] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

		/*[ScriptMethod(name: "第二轮远近刀_绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"], userControl: true)]
		public async void 第二轮远近刀_绘制(Event @event, ScriptAccessory accessory)
		{
			if (parse != 3.3) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			Vector3 center = new(100, 0, 100);

			//13-3-3-3
			await Task.Delay(1000);

			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制1A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 12000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制1B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 12000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制1C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 12000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制1D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 12000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			await Task.Delay(12000);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制3A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制3B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制3C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制3D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制4A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制4B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制4C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第二轮_远近刀_绘制4D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

		}*/

		[ScriptMethod(name: "魔法阵展开_四式_换P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43542"], userControl: false)]
		public void 魔法阵展开_四式_换P(Event @event, ScriptAccessory accessory)
		{
			parse = 7;
			远近刀记录 = [];
		}

		[ScriptMethod(name: "魔法阵展开_四式_地板收集", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void 魔法阵展开_四式_地板收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (index == 6)
			{
				Map4 = true;
			}
			if (EnableDev)
			{
				if (index == 5)
				{
					debugOutput = "南边放花";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
				if (index == 6)
				{
					debugOutput = "北边放花";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
			}
			//获取：内圈红（正北顺 04-0B）
			//6-花放北边-true
		}

		[ScriptMethod(name: "魔法阵展开_四式_点名收集", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"], userControl: false)]
		public void 魔法阵展开_四式_点名收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(tid);
			P3_4Mark[tIndex] = 1;
		}

		[ScriptMethod(name: "魔法阵展开_四式_点名指路", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"])]
		public void 魔法阵展开_四式_点名指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			if (tid != accessory.Data.Me) return;
			if (EnableDev)
			{
				debugOutput = "你要放花";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}

			var P4RotBase = Map4 ? 0 : float.Pi;
			if (myIndex == 0 || myIndex == 4)
			{
				var dealpos = RotatePoint(CloseBase, centre, P4RotBase + float.Pi / 8);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_四式_点名指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1 || myIndex == 5)
			{
				var dealpos = RotatePoint(CloseBase, centre, P4RotBase - float.Pi / 8);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_四式_点名指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2 || myIndex == 6)
			{
				var dealpos = RotatePoint(FarBase, centre, P4RotBase + float.Pi / 8);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_四式_点名指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3 || myIndex == 7)
			{
				var dealpos = RotatePoint(FarBase, centre, P4RotBase - float.Pi / 8);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "魔法阵展开_四式_点名指路";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "第三轮远近刀_远近记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"], userControl: false)]
		public void 第三轮远近刀_远近记录(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			远近刀记录.Add(@event.StatusParam == 759);
			if (EnableDev)
			{
				debugOutput = @event.StatusParam == 759 ? "远刀" : "近刀";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
		}

		[ScriptMethod(name: "第三轮远近刀_指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"])]
		public async void 第三轮远近刀_指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//13-3-3-3
			//从正西开始顺时针1-8
			List<Vector3> ClosePos = [];
			List<Vector3> FarPos = [];

			ClosePos.Add(new Vector3(100 - CloseMutli22_5, 0, 100 - CloseMutli67_5));
			ClosePos.Add(new Vector3(100 + CloseMutli22_5, 0, 100 - CloseMutli67_5));
			ClosePos.Add(new Vector3(100 - CloseMutli67_5, 0, 100 - CloseMutli22_5));
			ClosePos.Add(new Vector3(100 + CloseMutli67_5, 0, 100 - CloseMutli22_5));
			ClosePos.Add(new Vector3(100 - CloseMutli22_5, 0, 100 + CloseMutli67_5));
			ClosePos.Add(new Vector3(100 + CloseMutli22_5, 0, 100 + CloseMutli67_5));
			ClosePos.Add(new Vector3(100 - CloseMutli67_5, 0, 100 + CloseMutli22_5));
			ClosePos.Add(new Vector3(100 + CloseMutli67_5, 0, 100 + CloseMutli22_5));

			FarPos.Add(new Vector3(100 - FarMutli22_5, 0, 100 - FarMutli67_5));
			FarPos.Add(new Vector3(100 + FarMutli22_5, 0, 100 - FarMutli67_5));
			FarPos.Add(new Vector3(100 - FarMutli67_5, 0, 100 - FarMutli22_5));
			FarPos.Add(new Vector3(100 + FarMutli67_5, 0, 100 - FarMutli22_5));
			FarPos.Add(new Vector3(100 - FarMutli22_5, 0, 100 + FarMutli67_5));
			FarPos.Add(new Vector3(100 + FarMutli22_5, 0, 100 + FarMutli67_5));
			FarPos.Add(new Vector3(100 - FarMutli67_5, 0, 100 + FarMutli22_5));
			FarPos.Add(new Vector3(100 + FarMutli67_5, 0, 100 + FarMutli22_5));

			await Task.Delay(1000);


			if (myIndex < 4)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第三轮_远近刀_指路1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				await Task.Delay(12000);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第三轮_远近刀_指路2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = 远近刀记录[0] != 远近刀记录[1] ? ClosePos[myIndex] : FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第三轮_远近刀_指路3";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 3000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第三轮_远近刀_指路4";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = 远近刀记录[0] != 远近刀记录[1] ? FarPos[myIndex] : ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex > 3)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第三轮_远近刀_指路1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				await Task.Delay(12000);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第三轮_远近刀_指路2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = 远近刀记录[0] != 远近刀记录[1] ? FarPos[myIndex] : ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第三轮_远近刀_指路3";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 3000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "第三轮_远近刀_指路4";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = 远近刀记录[0] != 远近刀记录[1] ? ClosePos[myIndex] : FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}

		}

		/*[ScriptMethod(name: "第三轮远近刀_绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"], userControl: true)]
		public async void 第三轮远近刀_绘制(Event @event, ScriptAccessory accessory)
		{
			if (parse != 3.4) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			Vector3 center = new(100, 0, 100);

			//13-3-3-3
			await Task.Delay(5000);

			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制1A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 7000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制1B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 7000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制1C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 7000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制1D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[0] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 7000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

			await Task.Delay(7000);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制2A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[1] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制3A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制3B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制3C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制3D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[2] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 3000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制4A";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 1;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制4B";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制4C";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 3;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "第三轮_远近刀_绘制4D";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Position = center;
			dp.TargetResolvePattern = 远近刀记录[3] == false ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
			dp.TargetOrderIndex = 4;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 3000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
		}*/

		[ScriptMethod(name: "场外分身半场刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4318[45])$"])]
		public void 场外分身半场刀(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			if (EnableDev)
			{
				debugOutput = "注意左右刀";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "场外分身半场刀";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = @event["ActionId"] == "43185" ? float.Pi / 2 : float.Pi / -2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 6000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
		}

		[ScriptMethod(name: "魔法阵展开_五式_换P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43543"], userControl: false)]
		public void 魔法阵展开_五式_换P(Event @event, ScriptAccessory accessory)
		{
			parse = 8;
		}

		[ScriptMethod(name: "魔法阵展开_五式_刀刃收集", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4571"], userControl: false)]
		public void 魔法阵展开_五式_刀刃收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 8) return;
			var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
			if (P3_5Safe == 0)
			{
				if (pos.Z > 100)
				{
					P3_5Safe = 1;
				}
				if (pos.Z < 100)
				{
					P3_5Safe = 2;
				}			
				if (EnableDev)
				{
					if (P3_5Safe == 1)
					{
						debugOutput = "南安全";
						accessory.Method.SendChat($"""/e {debugOutput}""");
					}
					if (P3_5Safe == 2)
					{
						debugOutput = "北安全";
						accessory.Method.SendChat($"""/e {debugOutput}""");
					}
					if (P3_5Safe != 1 && P3_5Safe != 2)							
					{
						debugOutput = "有问题！";
						accessory.Method.SendChat($"""/e {debugOutput}""");
					}
				}
			}
			//1-南安全，2-北安全
		}

		[ScriptMethod(name: "魔法阵展开_五式_指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4344[89])$"])]
		public void 魔法阵展开_五式_指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 8) return;

			//8-先钢铁，9-先月环
			if (@event.ActionId == 43448)
			{
				if (P3_5Safe == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_五式_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 东上外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_五式_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 南左内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_五式_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 西下外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_五式_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 北右内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (@event.ActionId == 43449)
			{
				if (P3_5Safe == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_五式_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 东上内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_五式_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 南左外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_五式_指路1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 西下内;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_五式_指路2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = 北右外;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

		[ScriptMethod(name: "魔法阵展开_六式_换P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43544"], userControl: false)]
		public void 魔法阵展开_六式_换P(Event @event, ScriptAccessory accessory)
		{
			parse = 9;
		}

		[ScriptMethod(name: "魔法阵展开_六式_地板收集", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void 魔法阵展开_六式_地板收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 9) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (index == 6)
			{
				Map6 = true;
			}
			if (EnableDev)
			{
				debugOutput = index.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}

			//获取：右偏上内 红（05）/ 右偏下内 红（06）
			//06-true:MTD3 塔近7 花远8；STD4 花近1 塔远2；H1D1 花近5 塔远6；H2D2 塔近3 花远4
			//05-false:MTD3 花近8 塔远7；STD4 塔近2 花远1；H1D1 塔近6 花远5；H2D2 花近4 塔远3	
			// (2n-1)*pi/8
		}

		[ScriptMethod(name: "魔法阵展开_六式_点名收集", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"], userControl: false)]
		public void 魔法阵展开_六式_点名收集(Event @event, ScriptAccessory accessory)
		{
			if (parse != 9) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(tid);
			P3_6Mark[tIndex] = 1;
		}

		[ScriptMethod(name: "魔法阵展开_六式_点名指路", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"])]
		public void 魔法阵展开_六式_点名指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 9) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			if (tid != accessory.Data.Me) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P3_6Mark[myIndex] != 1) return;
			if (EnableDev)
			{
				debugOutput = "你要放花";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			if (myIndex == 0 || myIndex == 6)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 15 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 15 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 1 || myIndex == 7)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 1 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 1 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 2 || myIndex == 4)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 9 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 9 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 3 || myIndex == 5)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 7 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 7 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

		[ScriptMethod(name: "魔法阵展开_六式_塔指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43201"])]
		public void 魔法阵展开_六式_塔指路(Event @event, ScriptAccessory accessory)
		{
			if (parse != 9) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P3_6Mark[myIndex] == 1) return;
			if (EnableDev)
			{
				debugOutput = "你要踩塔";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}

			if (myIndex == 0 || myIndex == 6)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 13 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 13 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 1 || myIndex == 7)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 3 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 3 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 2 || myIndex == 4)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 11 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 11 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 3 || myIndex == 5)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 5 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 5 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "魔法阵展开_六式_点名指路";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
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
/*
		private byte? GetTransformationID(uint _id, ScriptAccessory accessory)
		{
			var obj = accessory.Data.Objects.SearchById(_id);
			if (obj != null)
			{
				unsafe
				{
					FFXIVClientStructs.FFXIV.Client.Game.Character.Character* objStruct = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)obj.Address;
					return objStruct->Timeline.ModelState;
				}
			}
			return null;
		}*/
		#endregion
	}
}
