using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritualGiftsTest.Interfaces;

public interface INavigationService
{
    Task<bool> NavigateAsync(string route);
    Task GoBackToRootAsync();
}
