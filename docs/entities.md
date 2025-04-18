- Category
    - Id
    - Name
    - ParentCategoryId

- Product
    - Name
    - Description
    - Image[]
    - CategoryId
    - Price
    - Attributes
        - Color
        - Size

 - Cart
    - UserId
    - Produtcs
        - ProductId
        - Quantity
        - Attributes
            - Color
            - Size

 - Orders
    - UserId
    - TotalPrice
    - OrderProduct[]
        - ProductId
        - Price
        - Quantity
        - Attributes
            - Color
            - Size
    - Status (Enum)
        - Pending
            - Placed
            - Confirmed
        - In Progress
            - Preparing
            - Packed
        - Completed
            - Delivered
            - Finished
        - Canceled
            - Customer Canceled
            - Out of Stock



# TODO
 - [] Seed products
 - [x] Checkout
 - [x] User Order
 - [x] Admin manage orders
 - [] Add shipping cost and shipping time 
 - [] Localize
 - [] Confirmation not working