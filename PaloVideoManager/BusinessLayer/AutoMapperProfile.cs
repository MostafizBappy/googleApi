using AutoMapper;
using PaloVideoManager.DataLayer;
using PaloVideoManager.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloVideoManager.BusinessLayer
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            Mapper.CreateMap<VideoSummary, VideoListViewModel>();
            Mapper.CreateMap<VideoListViewModel, VideoSummary>();
            Mapper.CreateMap<VideoManager, VideoManagerViewModel>();
            Mapper.CreateMap<VideoSummary, VideoSummaryViewModel>();
        }
    }
}