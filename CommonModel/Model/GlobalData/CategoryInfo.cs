using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    public class CategoryInfo
    {
        [Required(ErrorMessage = "필수")]
        public ReactiveProperty<int> CategoryId { get; set; } //DB Id
        public ReactiveProperty<string> OriginName { get; set; } // 기존 Name
        public ReactiveProperty<string> Name { get; set; } // 사용자지정 Name
        public ReactiveProperty<bool> IsChecked { get; set; }
        public CategoryInfo()
        {
            CategoryId = new ReactiveProperty<int>(0);
            OriginName = new ReactiveProperty<string>();
            Name = new ReactiveProperty<string>();
            IsChecked= new ReactiveProperty<bool>(true);
        }
        public CategoryInfo(int categoryId, string categoryName, string OriginName)
        {
            this.CategoryId.Value = categoryId;
            this.Name.Value = categoryName;
            this.OriginName.Value = OriginName;
            this.IsChecked.Value = true;
        }
        
    }
}
