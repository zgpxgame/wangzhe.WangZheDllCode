using System;

namespace behaviac
{
	internal class Assignment_bt_WrapperAI_Hero_HeroSimpleAI_node509 : Assignment
	{
		protected override EBTStatus update_impl(Agent pAgent, EBTStatus childStatus)
		{
			EBTStatus result = EBTStatus.BT_SUCCESS;
			int value = 1500;
			pAgent.SetVariable<int>("p_healthRate", value, 1780022873u);
			return result;
		}
	}
}
