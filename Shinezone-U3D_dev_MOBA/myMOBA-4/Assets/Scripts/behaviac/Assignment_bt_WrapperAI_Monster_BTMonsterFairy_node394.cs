using System;

namespace behaviac
{
	internal class Assignment_bt_WrapperAI_Monster_BTMonsterFairy_node394 : Assignment
	{
		protected override EBTStatus update_impl(Agent pAgent, EBTStatus childStatus)
		{
			EBTStatus result = EBTStatus.BT_SUCCESS;
			bool value = false;
			pAgent.SetVariable<bool>("p_forceToApproachHero", value, 2293816730u);
			return result;
		}
	}
}
