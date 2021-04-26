using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Menu;
using Assets.Scripts.Unity.UI_New.Knowledge;
using Assets.Scripts.Utils;
using BTD_Mod_Helper.Extensions;
using Harmony;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MegaKnowledge
{
    public class KnowledgeMenu
    {
        public static KnowledgeSkillTree knowledgeSkillTree;

        public static Dictionary<string, Dictionary<string, KnowledgeSkillBtn>> Buttons =
            new Dictionary<string, Dictionary<string, KnowledgeSkillBtn>>();

        [HarmonyPatch(typeof(KnowledgeSkillTree), nameof(KnowledgeSkillTree.UpdateButtonStates))]
        internal class KnowledgeSkillTree_UpdateButtonStates
        {
            [HarmonyPostfix]
            internal static void Postfix(KnowledgeSkillTree __instance)
            {
                knowledgeSkillTree = __instance;
                
                foreach (var (megaKnowledgeKey, megaKnowledge) in Main.MegaKnowledges)
                {
                    var cloneFrom = __instance.GetComponentsInChildren<Component>()
                        .First(component => component.name == megaKnowledge.cloneFrom).gameObject;

                    var child = cloneFrom.transform.parent.FindChild(megaKnowledge.name);
                    if (child != null)
                    {
                        return;
                    }

                    var newButton = Object.Instantiate(cloneFrom, cloneFrom.transform.parent, true);
                    newButton.transform.Translate(megaKnowledge.offsetX, megaKnowledge.offsetY, 0);

                    var btn = newButton.GetComponentInChildren<KnowledgeSkillBtn>();
                    newButton.name = megaKnowledge.name;
                    var knowledgeModels = Game.instance.model.knowledgeSets.First(model => model.name == megaKnowledge.set).GetKnowledgeModels();
                    btn.ClickedEvent = new Action<KnowledgeSkillBtn>(skillBtn =>
                    {
                        var hasAll = true;
                        var btd6Player = Game.instance.GetBtd6Player();
                        foreach (var knowledgeModel in knowledgeModels)
                        {
                            if (!btd6Player.HasKnowledge(knowledgeModel.id))
                            {
                                hasAll = false;
                            }
                        }
                        if (!(Input.GetKey(KeyCode.LeftShift) || hasAll))
                        {
                            foreach (var knowledgeSkillBtn in Buttons[megaKnowledge.set].Values)
                            {
                                knowledgeSkillBtn.SetState(KnowlegdeSkillBtnState.Available);
                            }

                            foreach (var mkv in Main.MegaKnowledges.Values.Where(mkv =>
                                mkv.set == megaKnowledge.set))
                            {
                                mkv.enabled = false;
                            }
                        }

                        if (Input.GetKey(KeyCode.LeftAlt))
                        {
                            megaKnowledge.enabled = false;
                            skillBtn.SetState(KnowlegdeSkillBtnState.Available);
                        }
                        else
                        {
                            megaKnowledge.enabled = true;

                            skillBtn.SetState(KnowlegdeSkillBtnState.Purchased);
                            knowledgeSkillTree.BtnClicked(skillBtn);
                            knowledgeSkillTree.selectedPanelTitleTxt.SetText(megaKnowledge.name);
                            knowledgeSkillTree.selectedPanelDescTxt.SetText(megaKnowledge.description);
                        }
                    });
                    btn.Construct(newButton);
                    if (!Buttons.ContainsKey(megaKnowledge.set))
                    {
                        Buttons[megaKnowledge.set] = new Dictionary<string, KnowledgeSkillBtn>();
                    }

                    Buttons[megaKnowledge.set][megaKnowledgeKey] = btn;
                    btn.SetState(megaKnowledge.enabled
                        ? KnowlegdeSkillBtnState.Purchased
                        : KnowlegdeSkillBtnState.Available);

                    var image = btn.GetComponentsInChildren<Component>().First(component => component.name == "Icon")
                        .GetComponent<Image>();

                    ResourceLoader.LoadSpriteFromSpriteReferenceAsync(new SpriteReference(megaKnowledge.GUID), image,
                        false);
                    image.mainTexture.filterMode = FilterMode.Trilinear;
                }

                foreach (var gameObject in knowledgeSkillTree.scrollers)
                {
                    gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 500);
                }
            }
        }

        [HarmonyPatch(typeof(KnowledgeSkillTree), nameof(KnowledgeSkillTree.Update))]
        internal class KnowledgeSkillTree_Update
        {
            [HarmonyPostfix]
            internal static void Postfix(KnowledgeSkillTree __instance)
            {
                foreach (var knowledgeSkillBtns in Buttons.Values)
                {
                    foreach (var megaKnowledge in knowledgeSkillBtns.Keys)
                    {
                        knowledgeSkillBtns[megaKnowledge].SetSelected(Main.MegaKnowledges[megaKnowledge].enabled);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(KnowledgeMain), nameof(KnowledgeMain.OnEnable))]
        internal class KnowledgeMain_Open
        {
            [HarmonyPostfix]
            internal static void Postfix(KnowledgeMain __instance)
            {
                var texts = new Dictionary<string, GameObject>
                {
                    {"Primary", __instance.primaryCompletedTxt.gameObject},
                    {"Military", __instance.militaryCompletedTxt.gameObject},
                    {"Magic", __instance.magicCompletedTxt.gameObject},
                    {"Support", __instance.supportCompletedTxt.gameObject},
                };

                foreach (var (set, text) in texts)
                {
                    var button = text.transform.parent.gameObject;
                    var existing = button.transform.FindChild(set);
                    if (existing != null)
                    {
                        continue;
                    }

                    var image = button.transform.FindChild("MKIcon").GetComponentInChildren<Image>().gameObject;
                    var i = 25;
                    foreach (var megaKnowledge in Main.MegaKnowledges.Values)
                    {
                        if (!megaKnowledge.enabled || megaKnowledge.set != set) continue;

                        var newImage = Object.Instantiate(image, button.transform, true);
                        //newImage.transform.Translate(i - 75, -150 - i / 3.6f, 0);
                        newImage.transform.Translate(-150f + i / 3.6f, i, 0);
                        newImage.transform.Rotate(0, 0 ,-15.5f);
                        newImage.name = set;

                        var realImage = newImage.GetComponent<Image>();

                        ResourceLoader.LoadSpriteFromSpriteReferenceAsync(new SpriteReference(megaKnowledge.GUID),
                            realImage, false);
                        realImage.mainTexture.filterMode = FilterMode.Trilinear;
                        i += 160;
                    }
                }
            }
        }
    }
}