using UrbanStreetz.Models.AppModels;
using UrbanStreetz.Repositories.Interfaces;
using UrbanStreetz.Repositories;
using UrbanStreetz.Context;
using System.Threading.Tasks;

namespace UrbanStreetz.Utilities
{
    public class StoryDeletionJob 
    {
        private readonly IStoryRepository _storyRepository;
        public StoryDeletionJob(IStoryRepository)
        {

        }
        public Task DeleteExpiredStories()
        {
            var storyRepo = new IStoryRepository();
            
            var dsf = _context.s
        }
    }
}
