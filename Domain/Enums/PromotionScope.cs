namespace MyShopServer.Domain.Enums;

// IMPORTANT:
// HotChocolate maps GraphQL enum names to CLR enum values. Keep the naming and
// underlying values stable to avoid incorrect scope values being deserialized.
public enum PromotionScope
{
 Product =0,
 Category =1,
 Order =2
}
