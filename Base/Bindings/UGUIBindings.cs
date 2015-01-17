using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

    public static class UGUIExtensions
    {


        public static IDisposable BindButtonToHandler(this GameObject gameObject, Button button, Action handler)
        {
            var d = button.AsClickObservable().Subscribe(_ => handler()).DisposeWith(gameObject);
            return d;
        }

        public static IDisposable BindButtonToHandler(this ViewBase viewBase, Button button, Action handler)
        {
            var d = button.AsClickObservable().Subscribe(_ => handler()).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindButtonToCommand<T>(this ViewBase viewBase, Button button, CommandBase<T> command)
        {
            var d = button.AsClickObservable().Subscribe(_ =>
            {
                    viewBase.ExecuteCommand(command);
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindButtonToCommand<T>(this ViewBase viewBase, Button button, CommandBase<T> command, Func<T> selector)
        {
            var d = button.AsClickObservable().Subscribe(_ =>
            {
                    viewBase.ExecuteCommand(command, selector());
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindInputFieldToProperty<TO>(this ViewBase viewBase, InputField inputField, P<TO> property, Func<string, TO> i2pSelector, Func<TO,string> p2iSelector)
        {
            var d1 = inputField.AsValueChangedObservable().Subscribe(value =>
            {
                property.OnNext(i2pSelector(value));
            });

            var d2 = property.Subscribe(value =>
            {
                inputField.text = p2iSelector(value);
            });

            return Disposable.Create(() =>
            {
                d1.Dispose();
                d2.Dispose();
            }).DisposeWith(viewBase);
        }

        public static IDisposable BindInputFieldToProperty(this ViewBase viewBase, InputField inputField, P<string> property)
        {
            var d1 = inputField.AsValueChangedObservable().Subscribe(value =>
            {
                    property.OnNext(value);
            }).DisposeWith(viewBase);

            var d2 = property.Subscribe(value =>
            {
                inputField.text = value;
            });

            return Disposable.Create(() =>
            {
                d1.Dispose();
                d2.Dispose();
            }).DisposeWith(viewBase);
        }

        public static IDisposable BindInputFieldToHandler(this ViewBase viewBase, InputField inputField, Action<string> handler)
        {
            var d = inputField.AsValueChangedObservable().Subscribe(value =>
            {
                handler(value);
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindInputFieldToCommand<T>(this ViewBase viewBase, InputField inputField, CommandBase<T> command)
        {
            var d = inputField.AsEndEditObservable().Subscribe(_ =>
            {
                viewBase.ExecuteCommand(command);
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindInputFieldToCommand<T>(this ViewBase viewBase, InputField inputField, CommandBase<T> command, Func<T> selector)
        {
            var d = inputField.AsEndEditObservable().Subscribe(_ =>
            {
                viewBase.ExecuteCommand(command, selector());
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindSliderToHandler(this ViewBase viewBase, Slider slider, Action<float> handler)
        {
            var d = slider.AsValueChangedObservable().Subscribe(value =>
            {
                handler(value);
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindScrollbarToHandler(this ViewBase viewBase, Scrollbar scrollBar, Action<float> handler)
        {
            var d = scrollBar.AsValueChangedObservable().Subscribe(value =>
            {
                handler(value);
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindScrollbarToProperty<TO>(this ViewBase viewBase, Scrollbar scrollbar, P<TO> property, Func<float, TO> i2pSelector, Func<TO, float> p2iSelector)
        {
            var d1 = scrollbar.AsValueChangedObservable().Subscribe(value =>
            {
                property.OnNext(i2pSelector(value));
            });

            var d2 = property.Subscribe(value =>
            {
                scrollbar.value = p2iSelector(value);
            });

            return Disposable.Create(() =>
            {
                d1.Dispose();
                d2.Dispose();
            }).DisposeWith(viewBase);
        }

        public static IDisposable BindScrollbarToProperty(this ViewBase viewBase, Scrollbar scrollbar, P<float> property)
        {
            var d1 = scrollbar.AsValueChangedObservable().Subscribe(value =>
            {
                property.OnNext(value);
            }).DisposeWith(viewBase);

            var d2 = property.Subscribe(value =>
            {
                scrollbar.value = value;
            });

            return Disposable.Create(() =>
            {
                d1.Dispose();
                d2.Dispose();
            }).DisposeWith(viewBase);
        }
        public static IDisposable BindSliderToProperty<TO>(this ViewBase viewBase, Slider slider, P<TO> property, Func<float, TO> i2pSelector, Func<TO, float> p2iSelector)
        {
            var d1 = slider.AsValueChangedObservable().Subscribe(value =>
            {
                property.OnNext(i2pSelector(value));
            });

            var d2 = property.Subscribe(value =>
            {
                slider.value = p2iSelector(value);
            });

            return Disposable.Create(() =>
            {
                d1.Dispose();
                d2.Dispose();
            }).DisposeWith(viewBase);
        }

        public static IDisposable BindSliderToProperty(this ViewBase viewBase, Slider slider, P<float> property)
        {
            var d1 = slider.AsValueChangedObservable().Subscribe(value =>
            {
                property.OnNext(value);
            }).DisposeWith(viewBase);

            var d2 = property.Subscribe(value =>
            {
                slider.value = value;
            });

            return Disposable.Create(() =>
            {
                d1.Dispose();
                d2.Dispose();
            }).DisposeWith(viewBase);
        }

        public static IDisposable BindToggleToHandler(this ViewBase viewBase, Toggle toggle, Action<bool> handler)
        {
            var d = toggle.AsValueChangedObservable().Subscribe(value =>
            {
                handler(value);
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindToggleToProperty<TO>(this ViewBase viewBase, Toggle toggle, P<TO> property, Func<bool, TO> i2pSelector, Func<TO, bool> p2iSelector)
        {
            var d1 = toggle.AsValueChangedObservable().Subscribe(value =>
            {
                property.OnNext(i2pSelector(value));
            });

            var d2 = property.Subscribe(value =>
            {
                toggle.isOn = p2iSelector(value);
            });

            return Disposable.Create(() =>
            {
                d1.Dispose();
                d2.Dispose();
            }).DisposeWith(viewBase);
        }

        public static IDisposable BindToggleToProperty(this ViewBase viewBase, Toggle toggle, P<bool> property)
        {
            var d1 = toggle.AsValueChangedObservable().Subscribe(value =>
            {
                property.OnNext(value);
            }).DisposeWith(viewBase);

            var d2 = property.Subscribe(value =>
            {
                toggle.isOn = value;
            });

            return Disposable.Create(() =>
            {
                d1.Dispose();
                d2.Dispose();
            }).DisposeWith(viewBase);
        }

        public static IDisposable BindToggleOnToCommand<T>(this ViewBase viewBase, Toggle toggle, CommandBase<T> command)
        {
            var d = toggle.AsValueChangedObservable().Subscribe(_ =>
            {
                viewBase.ExecuteCommand(command);
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindToggleOnToCommand<T>(this ViewBase viewBase, Toggle toggle, CommandBase<T> command, Func<T> selector)
        {
            var d = toggle.AsValueChangedObservable().Where(v=>v).Subscribe(_ =>
            {
                viewBase.ExecuteCommand(command, selector());
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindToggleOffToCommand<T>(this ViewBase viewBase, Toggle toggle, CommandBase<T> command)
        {
            var d = toggle.AsValueChangedObservable().Where(v => v == false).Subscribe(_ =>
            {
                viewBase.ExecuteCommand(command);
            }).DisposeWith(viewBase);
            return d;
        }

        public static IDisposable BindToggleOffToCommand<T>(this ViewBase viewBase, Toggle toggle, CommandBase<T> command, Func<T> selector)
        {
            var d = toggle.AsValueChangedObservable().Where(v=>v==false).Subscribe(_ =>
            {
                viewBase.ExecuteCommand(command, selector());
            }).DisposeWith(viewBase);
            return d;
        }
    
        public static IObservable<Unit> AsClickObservable(this Button button)
        {

            return Observable.Create<Unit>(observer =>
            {
                UnityAction unityAction = () => observer.OnNext(Unit.Default);
                button.onClick.AddListener(unityAction);
                return Disposable.Create(() => button.onClick.RemoveListener(unityAction));
            });
        }

        public static IObservable<string> AsEndEditObservable(this InputField inputField)
        {
            return Observable.Create<string>(observer =>
            {
                UnityAction<string> unityAction = observer.OnNext;
                inputField.onEndEdit.AddListener(unityAction);
                return Disposable.Create(() => inputField.onEndEdit.RemoveListener(unityAction));
            });
        }

        public static IObservable<string> AsValueChangedObservable(this InputField inputField)
        {
            return Observable.Create<string>(observer =>
            {
                UnityAction<string> unityAction = observer.OnNext;
                inputField.onValueChange.AddListener(unityAction);
                return Disposable.Create(() => inputField.onValueChange.RemoveListener(unityAction));
            });
        }

        public static IObservable<float> AsValueChangedObservable(this Slider slider)
        {
            return Observable.Create<float>(observer =>
            {
                UnityAction<float> unityAction = observer.OnNext;
                slider.onValueChanged.AddListener(unityAction);
                return Disposable.Create(() => slider.onValueChanged.RemoveListener(unityAction));
            });
        }
        public static IObservable<bool> AsValueChangedObservable(this Toggle toggle)
        {
            return Observable.Create<bool>(observer =>
            {
                UnityAction<bool> unityAction = observer.OnNext;
                toggle.onValueChanged.AddListener(unityAction);
                return Disposable.Create(() => toggle.onValueChanged.RemoveListener(unityAction));
            });
        }
    
        public static IObservable<float> AsValueChangedObservable(this Scrollbar scrollbar)
        {

            return Observable.Create<float>(observer =>
            {
                UnityAction<float> unityAction = observer.OnNext;
                scrollbar.onValueChanged.AddListener(unityAction);
                return Disposable.Create(() => scrollbar.onValueChanged.RemoveListener(unityAction));
            });
        }
        public static IObservable<PointerEventData> AsObservableOfClick(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.PointerClick).Cast<BaseEventData,PointerEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfDrag(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.Drag).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfBeginDrag(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.BeginDrag).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfEndDrag(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.EndDrag).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<AxisEventData> AsObservableOfMove(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.Move).Cast<BaseEventData, AxisEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfDrop(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.Drop).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfPointerDown(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.PointerDown).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfPointerUp(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.PointerUp).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfPointerEnter(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.PointerEnter).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfPointerExit(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.PointerExit).Cast<BaseEventData, PointerEventData>();
        }    
        public static IObservable<PointerEventData> AsObservableOfInitializePotentialDrag(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.InitializePotentialDrag).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<PointerEventData> AsObservableOfScroll(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.Scroll).Cast<BaseEventData, PointerEventData>();
        }
        public static IObservable<BaseEventData> AsObservableOfSelect(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.Select);
        }
        public static IObservable<BaseEventData> AsObservableOfSubmit(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.Submit);
        }
        public static IObservable<BaseEventData> AsObservableOfCancel(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.Cancel);
        }
        public static IObservable<BaseEventData> AsObservableOfDeselect(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.Deselect);
        }
        public static IObservable<BaseEventData> AsObservableOfUpdateSelected(this EventTrigger trigger)
        {
            return trigger.AsObservableOf(EventTriggerType.UpdateSelected);
        }  
        public static IObservable<BaseEventData> AsObservableOf(this EventTrigger trigger, EventTriggerType type)
        {
            return Observable.Create<BaseEventData>(observer =>
            {
                var entry = ComposeEntry(type, observer.OnNext);
                trigger.delegates.Add(entry);
                return Disposable.Create(() => trigger.delegates.Remove(entry));
            });
        }

        //Will the callback leak?
        private static EventTrigger.Entry ComposeEntry(EventTriggerType type, Action<BaseEventData> action)
        {
            var entry = new EventTrigger.Entry
            {
                eventID = type,
                callback = new EventTrigger.TriggerEvent()
            };
        
            entry.callback.AddListener(new UnityAction<BaseEventData>(action));

            return entry;
        }

    }
