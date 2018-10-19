using Assets.Scripts.GameLogic;
using System;
using UnityEngine;

namespace behaviac
{
	internal class Action_bt_WrapperAI_Soldier_BTSoldierPro_node17 : Action
	{
		protected override EBTStatus update_impl(Agent pAgent, EBTStatus childStatus)
		{
			Vector3 dest = (Vector3)pAgent.GetVariable(312907864u);
			((ObjAgent)pAgent).RealMovePosition(dest);
			return EBTStatus.BT_SUCCESS;
		}
	}
}
