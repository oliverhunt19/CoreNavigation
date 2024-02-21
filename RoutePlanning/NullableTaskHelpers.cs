using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutePlanning
{
    public static class NullableTaskHelpers
    {
        public static Task NullableTask(this Task? task)
        {
            return task.NullableTask(out _);
        }

        public static Task NullableTask(this Task? task, out bool IsNotNull)
        {
            if (task == null)
            {
                IsNotNull = false;
                return Task.CompletedTask;
            }
            else
            {
                IsNotNull = true;
                return task;
            }
        }

        public static Task<T?> NullableTask<T>(this Task<T?>? task)
        {
            return task.NullableTask(out _);
        }

        public static Task<T?> NullableTask<T>(this Task<T?>? task, out bool IsNotNull)
        {
            if (task == null)
            {
                IsNotNull = false;
                return Task.FromResult(default(T));
            }
            else
            {
                IsNotNull = true;
                return task;
            }
        }
    }
}
