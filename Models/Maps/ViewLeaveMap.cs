using FluentNHibernate.Mapping;
using HrInternWebApp.Models.Identity;

namespace HrInternWebApp.Models.Map
{
    public class ViewLeaveMap : SubclassMap<ViewLeave>
    {
        public ViewLeaveMap()
        {
            // Set the discriminator value if needed
            DiscriminatorValue("ViewLeave");

            // Map the additional properties specific to ViewLeave
            Map(x => x.Status).Column("status").Nullable();
            Map(x => x.Approver).Column("approver").Nullable();
        }
    }
}
