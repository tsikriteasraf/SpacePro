﻿using Entities.BroadClasses;
using Entities.IdentityUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Observer
{
    public abstract class NewsSubject
    {
        private List<NewsListener> _newsListeners;

        public NewsSubject(List<NewsListener> newsListeners)
        {
            _newsListeners = newsListeners;
        }

        public void AttachRangeListeners(List<NewsListener> listeners)
        {
            _newsListeners.AddRange(listeners);
        }

        public void DetachListener(NewsListener listener)
        {
            _newsListeners.Remove(listener);
        }

        private void Notify(Newsletter newsletter)
        {
            throw new NotImplementedException();
        }

        public void AddNewsletter(Newsletter newsletter)
        {
            throw new NotImplementedException();
        }
    }
}
