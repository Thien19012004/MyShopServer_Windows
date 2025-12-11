namespace MyShopServer.Domain.Enums
{
    public enum PaymentMethod
    {
        Cash = 0,          // Tiền mặt
        BankTransfer = 1,  // Chuyển khoản
        Card = 2,          // Thẻ
        EWallet = 3        // Ví điện tử (Momo, ZaloPay, ...)
    }
}
