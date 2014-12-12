﻿namespace Bolt {
  public interface IEventFilter {
    bool EventReceived(Event ev);
  }

  public class DefaultEventFilter : IEventFilter {
    bool IEventFilter.EventReceived(Event ev) {
      return true;
    }
  }
}