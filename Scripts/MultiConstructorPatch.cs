﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Objects;
using Assets.Scripts.Serialization;
using Assets.Scripts.Util;
using HarmonyLib;
using System;
using System.Xml.Serialization;
using static Assets.Scripts.Objects.Structure;
using UnityEngine;
using JetBrains.Annotations;
using System.Collections;
using System.IO;
using Assets.Scripts.Inventory;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.GridSystem;

namespace ChangeStructuresAddon.Scripts
{
    [HarmonyPatch(typeof(MultiConstructor))]
	public class MultiConstructorPatch
    {
		[HarmonyPatch("Construct", new Type[] { typeof(Grid3), typeof(Quaternion), typeof(int), typeof(Item), typeof(bool), typeof(ulong), typeof(Mothership), typeof(int) })]
		[HarmonyPrefix]
		public static void ConstructPrefix(MultiConstructor __instance, Grid3 localPosition, Quaternion targetRotation, int optionIndex, Item offhandItem, bool authoringMode, ulong steamId, Mothership mothership, int quantity)
		{
			Debug.Log("ChangeStructuresAddon: Reading StructureEditList:");
			StructureEdits edits = ChangeStructuresAddon.CachedStructureEdits;

			if (edits == null)
			{
				Debug.Log("ChangeStructuresAddon: ERROR: StructureEdits objects in XML is null");
				return;
			}

			Debug.Log(edits.StructureEditList);
			Debug.Log("ChangeStructuresAddon: Read StructureEditList:");

			if (__instance.Constructables == null)
				return;

			Debug.Log("ChangeStructuresAddon: constructable found");
			int editIdx = edits.StructureEditList.FindIndex(x => x.StructurePrefabName == __instance.Constructables[optionIndex].PrefabName);
			Debug.Log("ChangeStructuresAddon: constructable found");
			Debug.Log(editIdx);
			Debug.Log("ChangeStructuresAddon: edit");

			if (editIdx != -1)
			{
				StructureEdit edit = edits.StructureEditList[editIdx];

				Debug.Log("ChangeStructuresAddon: Found structure to edit:");
				Debug.Log(edit.StructurePrefabName);

				foreach (BuildStateData buildStateData in edit.BuildStateDataList)
				{
					int stateIdx = buildStateData.State;
					BuildState editedBuildState = __instance.Constructables[optionIndex].BuildStates[stateIdx];
					if (editedBuildState != null)
					{
						float newEntryTime = buildStateData.ToolUseData.EntryTime;
						string newToolEntryName = buildStateData.ToolUseData.ToolEntryPrefabName;
						string newToolEntry2Name = buildStateData.ToolUseData.ToolEntry2PrefabName;
						int newEntryQuantity = buildStateData.ToolUseData.EntryQuantity;
						int newEntryQuantity2 = buildStateData.ToolUseData.EntryQuantity2;

						Debug.Log("ChangeStructuresAddon: Found buildstate:");
						Debug.Log(editedBuildState);

						Debug.Log("ChangeStructuresAddon: XML buildstate:");
						Debug.Log(newEntryTime);
						Debug.Log(newToolEntryName);
						Debug.Log(newToolEntry2Name);
						Debug.Log(newEntryQuantity);
						Debug.Log(newEntryQuantity2);

						if (newEntryTime != float.NaN)
						{
							Debug.Log("ChangeStructuresAddon: Changed EntryTime from " + editedBuildState.Tool.EntryTime + " to " + newEntryTime);
							editedBuildState.Tool.EntryTime = newEntryTime;
						}
						if (newToolEntryName != "")
						{
							Item newItem = Item.FindPrefab(newToolEntryName) as Item;
							Debug.Log("ChangeStructuresAddon: Changed ToolEntry " + editedBuildState.Tool.ToolEntry + " to " + newItem);
							editedBuildState.Tool.ToolEntry = newItem;
						}
						if (newToolEntry2Name != "")
						{
							Item newItem = Item.FindPrefab(newToolEntry2Name) as Item;
							Debug.Log("ChangeStructuresAddon: Changed ToolEntry2 " + editedBuildState.Tool.ToolEntry2 + " to " + newItem);
							editedBuildState.Tool.ToolEntry2 = newItem;
						}
						if (newEntryQuantity != -1)
						{
							Debug.Log("ChangeStructuresAddon: Changed EntryQuantity " + editedBuildState.Tool.EntryQuantity + " to " + newEntryQuantity);
							editedBuildState.Tool.EntryQuantity = newEntryQuantity;
						}
						if (newEntryQuantity2 != -1)
						{
							Debug.Log("ChangeStructuresAddon: Changed EntryQuantity2 " + editedBuildState.Tool.EntryQuantity2 + " to " + newEntryQuantity2);
							editedBuildState.Tool.EntryQuantity2 = newEntryQuantity2;
						}
					}

					__instance.Constructables[optionIndex].BuildStates[stateIdx] = editedBuildState;
					//(InventoryManager.PrecisionPlaceCursor).BuildStates[stateIdx] = editedBuildState;
					Debug.Log("ChangeStructuresAddon: Edited constructable " + edit.StructurePrefabName);
				}
			}
		}
	}
}
