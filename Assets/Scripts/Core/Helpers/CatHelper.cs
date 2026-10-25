namespace UyuyanKopekler.Core
{
    // Kedi yardımcısı: bir çim kareye gönderilir; o kare ve 8 komşusundaki köpekleri
    // ortaya çıkarır, kare güvenliyse kazar. Bedeli kemikle ödenir.
    public static class CatHelper
    {
        public const int BonePrice = 10;

        public static bool CanAfford(Inventory inventory) => inventory.Count(ItemType.Bone) >= BonePrice;

        public static bool TryPay(Inventory inventory) => inventory.TrySpend(ItemType.Bone, BonePrice);
    }
}
