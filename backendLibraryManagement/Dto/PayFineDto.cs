namespace backendLibraryManagement.Dto
{
    // MVP: “pay” just confirms the action.
    // Later you can add provider fields: transactionId, paymentMethod, etc.
    public class PayFineDto
    {
        public int UserId { get; set; }
    }
}
