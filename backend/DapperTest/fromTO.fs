module dapper_test.fromTO

type OrderStatusDto = {
    OrderId: Guid
    Status: string
    ShippedDate: DateTime option
    CancelledReason: string option
}

let toDomain (dto: OrderStatusDto): OrderStatus =
    match dto.Status with
    | "Pending" -> Pending
    | "Shipped" -> Shipped dto.ShippedDate.Value
    | "Cancelled" -> Cancelled dto.CancelledReason.Value
    | "Delivered" -> Delivered
    | status -> failwith $"Unknown status: {status}"

let fromDomain (orderId: Guid) (status: OrderStatus): OrderStatusDto =
    match status with
    | Pending -> { OrderId = orderId; Status = "Pending"; ShippedDate = None; CancelledReason = None }
    | Shipped date -> { OrderId = orderId; Status = "Shipped"; ShippedDate = Some date; CancelledReason = None }
    | Cancelled reason -> { OrderId = orderId; Status = "Cancelled"; ShippedDate = None; CancelledReason = Some reason }
    | Delivered -> { OrderId = orderId; Status = "Delivered"; ShippedDate = None; CancelledReason = None }

let getOrderStatus (conn: SqlConnection) (orderId: Guid) =
    conn.QuerySingleAsync<OrderStatusDto>(
        "SELECT OrderId, Status, ShippedDate, CancelledReason FROM Orders WHERE OrderId = @OrderId",
        {| OrderId = orderId |})
    |> Async.AwaitTask
    |> Async.map toDomain