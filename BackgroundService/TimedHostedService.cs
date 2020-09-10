using BlogHost.services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlogHost.BackgroundService
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedHostedService> _logger;
        private Timer _timer;

        public TimedHostedService(ILogger<TimedHostedService> logger, IConfiguration configuration)
        {
            _logger = logger;

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }


        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromHours(72));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            EmailService emailService = new EmailService();
            await emailService.SendEmailAsync(Configuration["Email:mail"], Configuration["Email:password"], "New posts", $"Вы давно не заходили к нам на сайт,не желаете взглянуть на новые статьи? Тогда перейдите по ссылке :" +
                $"<a href='https://localhost:44307/'>link</a> ");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);//вызываем повторно

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
