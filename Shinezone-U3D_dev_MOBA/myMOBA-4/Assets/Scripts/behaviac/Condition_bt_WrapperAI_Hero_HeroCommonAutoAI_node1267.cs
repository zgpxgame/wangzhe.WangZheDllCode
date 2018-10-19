using Assets.Scripts.GameLogic;
using System;

namespace behaviac
{
	internal class Condition_bt_WrapperAI_Hero_HeroCommonAutoAI_node1267 : Condition
	{
		protected override EBTStatus update_impl(Agent pAgent, EBTStatus childStatus)
		{
			uint objID = (uint)pAgent.GetVariable(2561624163u);
			bool flag = ((ObjAgent)pAgent).IsTargetCanBeAttacked(objID);
			bool flag2 = true;
			bool flag3 = flag == flag2;
			return (!flag3) ? EBTStatus.BT_FAILURE : EBTStatus.BT_SUCCESS;
		}
	}
}
