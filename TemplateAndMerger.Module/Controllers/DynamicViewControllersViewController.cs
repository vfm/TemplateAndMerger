using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Templates;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplateAndMerger.Module.Interfaces;

namespace TemplateAndMerger.Module.Controllers
{
    public class DynamicViewControllersViewController : ViewController
    {
        private List<Type> ctypelist = new List<Type>();
        private bool registered = false;

        public DynamicViewControllersViewController()
        {
            BuildControllerList();
        }

        public virtual void BuildControllerList()
        {
            var h = XafTypesInfo.Instance.PersistentTypes.Where(pt => pt.FindAttribute<MergeableAttribute>() != null);

            foreach (ITypeInfo item in h)
            {
                Type t = item.Type;
                Type generic = typeof(Merger<>);
                Type genericType = generic.MakeGenericType(new System.Type[] { t });
                Type genericBasicMergerVC = typeof(MergerViewController<>);
                Type genericMergerVC = genericBasicMergerVC.MakeGenericType(new System.Type[] { t });
                TypeList.Add(genericMergerVC);
                Type genericBaseControllerType = typeof(MergerGenericViewController<,>);
                Type genericControllerType = genericBaseControllerType.MakeGenericType(new Type[] { t, genericType });
                TypeList.Add(genericControllerType);
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            if (Frame.IsViewControllersActivation)
            {
                Frame.ViewControllersActivated += Frame_ViewControllersActivated;
            }
            else
            {
                CreateViewControllers();
            }
        }

        protected void Frame_ViewControllersActivated(object sender, EventArgs e)
        {
            CreateViewControllers();
        }

        private void CreateViewControllers()
        {
            if (!registered)
            {
                if (TypeList.Count > 0)
                {
                    foreach (Type type in TypeList)
                    {
                        ViewController c = (ViewController)Activator.CreateInstance(type);
                        Frame.RegisterController(c);
                    }
                }
                registered = true;
            }
        }

        private void RemoveViewControllers()
        {
            if (registered)
            {
                if (TypeList.Count > 0)
                {
                    foreach (Type type in TypeList)
                    {
                        Frame.Controllers.Remove(type);
                    }
                }
            }
        }

        protected override void OnDeactivated()
        {
            RemoveViewControllers();

            base.OnDeactivated();
        }

        public List<Type> TypeList
        {
            get
            {
                return ctypelist;
            }
            set
            {
                ctypelist = value;
            }
        }
    }

    abstract public class MergerListViewController : ViewController<ListView>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            if (View.Id.EndsWith("_ListToMerge_ListView"))
            {
                foreach (var controller in Frame.Controllers)
                    if (!(controller is DevExpress.ExpressApp.SystemModule.LinkUnlinkController))
                    {
                        foreach (var action in controller.Actions)
                            action.Active.SetItemValue("MergerActive", false);
                    }

                View.ControlsCreated += View_ControlsCreated;
            }
        }

        private void View_ControlsCreated(object sender, EventArgs e)
        {
            ResetFilter();
        }

        public abstract void ResetFilter();
    }

    public class MergerViewController<T> : ViewController where T : XPBaseObject
    {
        public MergerViewController()
        {
            TargetObjectType = typeof(IMerger<T>);
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

            ListPropertyEditor lpe = ((DetailView)View).FindItem("ListToMerge") as ListPropertyEditor;
            if (lpe.ListView != null)
            {
                if (lpe.ListView.Editor != null)
                {
                    if (lpe.ListView.Editor.Control != null)
                    {
                        CustomizeListEditor(lpe.ListView.Editor);
                    }
                    else
                    {
                        lpe.ListView.Editor.ControlsCreated += Editor_ControlsCreated;
                    }
                }
            }
        }

        void Editor_ControlsCreated(object sender, EventArgs e)
        {
            ListEditor le = sender as ListEditor;
            le.ControlsCreated -= Editor_ControlsCreated;
            CustomizeListEditor(le);
        }

        public virtual void CustomizeListEditor(ListEditor lpe)
        {
            lpe.FocusedObjectChanged += lpe_FocusedObjectChanged;
        }

        void lpe_FocusedObjectChanged(object sender, EventArgs e)
        {
            ListEditor h = sender as ListEditor;
            if (h != null)
            {
                var cur = View.CurrentObject as IMerger<T>;
                if (cur != null)
                {
                    cur.WinnerObject = h.FocusedObject as T;
                }
            }
        }
    }

    public class MergerGenericViewController<T, U> : ViewController
        where U : IMerger<T>
        where T : XPBaseObject
    {
        PopupWindowShowAction pswMerger;
        PopupWindowShowAction pswMergerContextMenu;

        public MergerGenericViewController()
        {
            pswMerger = new PopupWindowShowAction(this, "Zusammenführen_" + typeof(T).Name, DevExpress.Persistent.Base.PredefinedCategory.Tools);
            pswMerger.SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects;
            pswMerger.Caption = "Zusammenführen";
            pswMerger.ImageName = "Action_Debug_Start";
            pswMerger.PaintStyle = ActionItemPaintStyle.CaptionAndImage;
            pswMerger.TargetObjectType = typeof(T);
            pswMerger.TargetViewType = ViewType.ListView;
            pswMerger.TargetViewNesting = Nesting.Any;
            pswMerger.Execute += pswMerger_Execute;
            pswMerger.CustomizePopupWindowParams += pswMerger_CustomizePopupWindowParams;

            pswMergerContextMenu = new PopupWindowShowAction(this, "Zusammenführen_KontextMenu_" + typeof(T).Name, DevExpress.Persistent.Base.PredefinedCategory.Menu);
            pswMergerContextMenu.SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects;
            pswMergerContextMenu.Caption = "Zusammenführen";
            pswMergerContextMenu.ImageName = "Action_Debug_Start";
            pswMergerContextMenu.TargetObjectType = typeof(T);
            pswMergerContextMenu.TargetViewType = ViewType.ListView;
            pswMergerContextMenu.Execute += pswMerger_Execute;
            pswMergerContextMenu.CustomizePopupWindowParams += pswMerger_CustomizePopupWindowParams;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            if (View.Id.EndsWith("_ListToMerge_ListView"))
                pswMergerContextMenu.Active.SetItemValue("MergerContextActive", false);
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            {
                if (Frame != null)
                {
                    if (Frame.Template != null)
                    {
                        IActionContainer iac = Frame.Template.GetContainers().FirstOrDefault<IActionContainer>(c => c.ContainerId == pswMerger.Category);
                        if (iac != null)
                        {
                            iac.Register(pswMerger);
                        }

                        iac = Frame.Template.GetContainers().FirstOrDefault<IActionContainer>(c => c.ContainerId == pswMergerContextMenu.Category);
                        if (iac != null)
                        {
                            iac.Register(pswMergerContextMenu);
                        }
                    }
                }
            }
        }

        public void pswMerger_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            if (View.ObjectSpace != null)
            {
                U h = Activator.CreateInstance<U>();
                var lb = new List<T>();
                foreach (T item in View.SelectedObjects)
                {
                    lb.Add(item);
                }
                h.ListToMerge = lb;
                e.View = Application.CreateDetailView(View.ObjectSpace, h, false);
                e.DialogController.SaveOnAccept = false;
                e.DialogController.AcceptAction.ConfirmationMessage = "Wichtiger Hinweis: Aufgrund der nicht mehr rückgängig zu machenden Auswirkungen empfehlen wir ggf. Rücksprache " +
                    "mit dem Support und/oder eine Datensicherung vor Datensatz-Zusammenführungen zu erstellen. \nSind Sie sicher, dass der markierte Datensatz " +
                    "alle anderen Datensätze der Tabelle ersetzen soll?";
            }
            else throw new UserFriendlyException("Beim Zusammenführen ist ein Fehler aufgetreten. Bitte schließen Sie das aktuelle Fenster und versuchen Sie es erneut!");
        }

        public void pswMerger_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            U h = (U)e.PopupWindow.View.CurrentObject;
            if (h != null)
            {
                h.MergeObjects();
                View.ObjectSpace.CommitChanges();
            }
            ObjectSpace.CommitChanges();
            View.ObjectSpace.Refresh();
        }
    }
}