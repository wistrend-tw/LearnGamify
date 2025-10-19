using System;
using System.Collections.Generic;

namespace WisLife.Models;

public partial class Store
{
    public int Id { get; set; }

    /// <summary>
    /// 任務清單ID
    /// </summary>
    public string TabType_Id { get; set; } = null!;

    /// <summary>
    /// Commodity.Id 商品ID
    /// </summary>
    public int CommodityId { get; set; }

    /// <summary>
    /// 商品標題
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 擁有數量
    /// </summary>
    public int OwnedAmount { get; set; }

    /// <summary>
    /// 蓋買所需金幣
    /// </summary>
    public int RequiredCoins { get; set; }

    /// <summary>
    /// 商品 Icon Url
    /// </summary>
    public string IconCode { get; set; } = null!;

    /// <summary>
    /// 是否可以合成 false=否,1=true
    /// </summary>
    public string IsComposite { get; set; } = null!;
}
