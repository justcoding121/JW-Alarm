using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using System.IO;
using Newtonsoft.Json;

namespace JW.Alarm.Services
{
    public abstract class ScheduleService : IScheduleService
    {
        private Random random = new Random();
        private IStorageService storageService;
        private Lazy<string> scheduleFilePath;
        private static Dictionary<int, AlarmSchedule> schedules;

        public Task<Dictionary<int, AlarmSchedule>> Schedules => getSchedules();

        public ScheduleService(IStorageService storageService)
        {
            this.storageService = storageService;
            scheduleFilePath = new Lazy<string>(() => Path.Combine(this.storageService.StorageRoot, "schedules.json"));
        }

        public virtual async Task Create(AlarmSchedule schedule)
        {
            await getSchedules();

            schedule.Id = getNextId();
            schedules.Add(schedule.Id, schedule);

            await saveChanges();
        }

        public virtual async Task Delete(int scheduleId)
        {
            await getSchedules();

            schedules.Remove(scheduleId);

            await saveChanges();
        }

        public virtual async Task Update(AlarmSchedule schedule)
        {
            await getSchedules();

            schedules[schedule.Id] = schedule;

            await saveChanges();
        }

        private async Task<Dictionary<int, AlarmSchedule>> getSchedules()
        {
            if (schedules == null)
            {
                if(await storageService.FileExists(scheduleFilePath.Value))
                {
                    schedules = JsonConvert.DeserializeObject<Dictionary<int, AlarmSchedule>>(await storageService.ReadFile(scheduleFilePath.Value));
                }
                
                if(schedules == null)
                {
                    schedules = new Dictionary<int, AlarmSchedule>();
                }
            }

            return schedules;
        }

        private async Task saveChanges()
        {
            await storageService.SaveFile(storageService.StorageRoot, "schedules.json", JsonConvert.SerializeObject(schedules));
        }

        private int getNextId()
        {
            var candidate = random.Next();

            while(schedules.ContainsKey(candidate))
            {
                candidate = random.Next();
            }

            return candidate;
        }
    }
}
