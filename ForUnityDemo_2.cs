using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using System;
using Mono.Data.Sqlite;

public class InitCharacter : MonoBehaviour
{
    public void Awake()
    {
        xmlStructLoad = XmlStructLoad.GetInstance();
    }

    // Use this for initialization
    void Start()
    {
        spawnPool = PoolManager.Pools["SkillPool"];
        spineSkeAnimation = transform.GetComponent<SkeletonAnimation>();
        damageFormula = transform.GetComponent<DamageFormula>();

		if (transform.name.Equals("Character"))
		{
			string skeletondataAsset = FightingManager.db.searchMultiCondition("SpineID_SkeletonData", "PJ_Party", "Party_1_or_2", "=", "1", "Party_Number", "=", PlayerPrefs.GetInt("AutoSelectParty").ToString());

			if (skeletondataAsset != null)
			{
				//Spine初始設置
				spineSkeAnimation.skeletonDataAsset = LoadAssetBundles.CharacterAssetbundle.LoadAsset<SkeletonDataAsset>(skeletondataAsset);
				MeshRenderer objMeshRenderer = transform.GetComponent<MeshRenderer>();
				objMeshRenderer.sortingLayerName = "Scene02";
				objMeshRenderer.sortingOrder = 2;
				spineSkeAnimation.name = skeletondataAsset;
				spineSkeAnimation.loop = true;
				spineSkeAnimation.Reset();
				//++++++++++

				//添加動畫相關事件
				Spine.AnimationState spineSkeState = spineSkeAnimation.state;
				spineSkeState.SetAnimation(0, "stand", true);
				spineSkeState.Complete += OnAnimationComplete;
				spineSkeState.Event += OnEvent;
				spineSkeAnimation.skeleton.SetToSetupPose();
				//++++++

				//TJ 單機版修改
				string heroType = xmlStructLoad.hero_type_Configuration.Heros.Hero.Find(x => x.Portrait_s == skeletondataAsset).Id;
				List<XmlStruct.Hero_have_skill> heroHaveSkill = xmlStructLoad.hero_have_skill_Configuration.Hero_have_skills.Hero_have_skill.FindAll(x => x.Hero_type == heroType);
				Array.Resize (ref skillType, heroHaveSkill.Count);
				for (int i = 0; i < heroHaveSkill.Count; i++)
				{
					skillType[i] = heroHaveSkill[i].Skill_type;
				}

				SqliteDataReader reader;
				reader = FightingManager.db.searchFullTableCondition("PJ_HeroData", "Hero_Type", "=", heroType);
				int physicalAttack = 0;
				int magicAttack = 0;
				if (reader != null)
				{
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							physicalAttack = int.Parse(reader[6].ToString()) * 3;
							magicAttack = int.Parse(reader[7].ToString()) * 3;
						}
						reader.Close();
					}
				}
				damageFormula.SetAttackFunction(physicalAttack);
				damageFormula.SetMagicFunction(magicAttack);
				//+++++++++++++++++++++++++++++++++++++++++++++++++
			}
			else if (skeletondataAsset == null)
			{
				img_UIButtonEvent.SendMessage("CloseBigSkill");
				Character2P.enabled = false;
				gameObject.SetActive(false);
			}
		}
    }

    //Spine 動畫觸發事件
    void OnEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
    {
		Spine.Skeleton spineSke = spineSkeAnimation.skeleton;
        if (e.ToString() == "attack1")
        {
            AttackEventFunction("hit");
        }
        else if (e.ToString() == "attack2")
        {
            AttackEventFunction("hit2");
        }
        else if (e.ToString() == "attack3")
        {
            AttackEventFunction("hit3");
        }
        else if (e.ToString() == "skill1")
        {
            SkillEventFunction(0);
        }
        else if (e.ToString() == "skill2")
        {
            SkillEventFunction(1);
        }
        else if (e.ToString() == "skill3")
        {
            SkillEventFunction(2);
        }
		//減益
        else if (e.ToString() == "dof")
        {
            RaycastHit2D[] hits;
            Debug.DrawRay(transform.position, Vector2.right * 6, Color.green);
			if (!spineSke.flipX)
                hits = Physics2D.RaycastAll(transform.position, Vector2.right, 6.0F);
            else
                hits = Physics2D.RaycastAll(transform.position, Vector2.left, 6.0F);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.transform.tag == "Monster")
                {
                    hit.transform.gameObject.AddComponent<LessBlood_Dof>(); //持續扣血
                }
            }
        }
		//++++++++

		//增益
        else if (e.ToString() == "buff")
        {
            RaycastHit2D[] hits;
            Debug.DrawRay(transform.position, Vector2.right * 6, Color.green);
			if (!spineSke.flipX)
                hits = Physics2D.RaycastAll(transform.position, Vector2.right, 6.0F);
            else
                hits = Physics2D.RaycastAll(transform.position, Vector2.left, 6.0F);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.transform.tag == "Monster")
                {
                    if (hit.transform.gameObject.GetComponent<Giddy_Buff>() == null)
                    {
                        hit.transform.gameObject.AddComponent<Giddy_Buff>(); //暈眩
                    }
                }
            }
        }
		//++++++++++
    }
}
