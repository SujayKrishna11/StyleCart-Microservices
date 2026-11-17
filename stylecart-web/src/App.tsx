import { useEffect, useMemo, useState, type FormEvent } from "react";
import "./App.css";

type Product = { id: number; name: string; description: string | null; brand: string | null; category: string; imageUrl: string | null; basePrice: number; isInStock: boolean };
type Variant = { id: number; productId: number; color: string; size: string; price: number; quantityInStock: number; isInStock: boolean };
type Recommendation = { productVariantId: number; productName: string; variantDescription: string; unitPrice: number };
type CartItem = { id: number; productVariantId: number; productName: string; variantDescription: string; unitPrice: number; quantity: number; lineTotal: number };
type Cart = { id: number; updatedAt: string; items: CartItem[] };
type Order = { id: number; orderDate: string; totalAmount: number; orderStatus: string; recipientName: string; shippingAddress: string; items: CartItem[] };
type AuthResponse = { token: string; email: string };
type SupportResponse = { reply: string; mode: string };
type Page = "catalog" | "cart" | "checkout" | "orders";
type AuthMode = "login" | "register";

async function request<T>(url: string, options: RequestInit = {}): Promise<T> {
    const response = await fetch(url, options);
    if (!response.ok) throw new Error((await response.text()) || `Request failed with status ${response.status}.`);
    return response.status === 204 ? undefined as T : response.json() as Promise<T>;
}

function App() {
    const [page, setPage] = useState<Page>("catalog");
    const [authMode, setAuthMode] = useState<AuthMode>("login");
    const [products, setProducts] = useState<Product[]>([]);
    const [searchText, setSearchText] = useState("");
    const [selectedCategory, setSelectedCategory] = useState("All");
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
    const [variants, setVariants] = useState<Variant[]>([]);
    const [selectedVariantId, setSelectedVariantId] = useState<number | null>(null);
    const [quantity, setQuantity] = useState(1);
    const [alsoBought, setAlsoBought] = useState<Recommendation[]>([]);
    const [forYou, setForYou] = useState<Recommendation[]>([]);
    const [cart, setCart] = useState<Cart | null>(null);
    const [orders, setOrders] = useState<Order[]>([]);
    const [supportMessage, setSupportMessage] = useState("");
    const [supportReply, setSupportReply] = useState<SupportResponse | null>(null);
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [recipientName, setRecipientName] = useState("");
    const [shippingAddress, setShippingAddress] = useState("");
    const [token, setToken] = useState(localStorage.getItem("styleCartToken") ?? "");
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    const headers = () => ({ Authorization: `Bearer ${token}` });
    const cartItemCount = useMemo(() => cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0, [cart]);
    const cartTotal = useMemo(() => cart?.items.reduce((sum, item) => sum + item.lineTotal, 0) ?? 0, [cart]);
    const selectedVariant = variants.find((variant) => variant.id === selectedVariantId);
    const categories = useMemo(() => ["All", ...Array.from(new Set(products.map((product) => product.category))).sort()], [products]);
    const visibleProducts = useMemo(() => selectedCategory === "All" ? products : products.filter((product) => product.category === selectedCategory), [products, selectedCategory]);
    const showError = (reason: unknown, fallback: string) => setError(reason instanceof Error ? reason.message : fallback);

    useEffect(() => { void loadProducts(); }, []);
    useEffect(() => { if (token) { void loadCart(); void loadForYou(); } else { setCart(null); setForYou([]); } }, [token]);
    useEffect(() => {
        if (!message) return;
        const timer = window.setTimeout(() => setMessage(""), 3500);
        return () => window.clearTimeout(timer);
    }, [message]);

    async function loadProducts(search = "") {
        try { setLoading(true); setError(""); setProducts(await request<Product[]>(`/gateway/products${search ? `?search=${encodeURIComponent(search)}` : ""}`)); }
        catch (reason) { showError(reason, "Could not load products."); }
        finally { setLoading(false); }
    }

    async function loadCart() {
        if (!token) return;
        try { setCart(await request<Cart>("/gateway/cart", { headers: headers() })); }
        catch (reason) { showError(reason, "Could not load your cart."); }
    }

    async function loadForYou() {
        try { setForYou(await request<Recommendation[]>("/gateway/recommendations/for-you", { headers: headers() })); }
        catch { setForYou([]); }
    }

    async function loadOrders() {
        if (!token) return;
        try { setLoading(true); setOrders(await request<Order[]>("/gateway/orders/my-orders", { headers: headers() })); }
        catch (reason) { showError(reason, "Could not load your orders."); }
        finally { setLoading(false); }
    }

    async function handleSearch(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        const search = searchText.trim();
        if (search.length === 1) { setError("Enter at least two characters to search."); return; }
        await loadProducts(search);
    }

    async function selectProduct(product: Product, preferredVariantId?: number) {
        try {
            setSelectedProduct(product); setVariants([]); setSelectedVariantId(null); setAlsoBought([]); setQuantity(1); setError("");
            const items = await request<Variant[]>(`/gateway/products/${product.id}/variants`);
            setVariants(items);
            setSelectedVariantId(items.find((item) => item.id === preferredVariantId && item.isInStock)?.id ?? items.find((item) => item.isInStock)?.id ?? null);
            if (items[0]) setAlsoBought(await request<Recommendation[]>(`/gateway/recommendations/variants/${items[0].id}/also-bought`));
            window.setTimeout(() => document.getElementById("product-details")?.scrollIntoView({ behavior: "smooth", block: "start" }), 0);
        } catch (reason) { showError(reason, "Could not load product details."); }
    }

    function openRecommendedProduct(item: Recommendation) {
        const product = products.find((currentProduct) => currentProduct.name === item.productName);
        if (!product) { setError("Could not find this recommended product."); return; }
        void selectProduct(product, item.productVariantId);
    }

    async function addRecommendedProduct(item: Recommendation) {
        if (!token) { setMessage("Please sign in before adding products to your cart."); document.getElementById("account")?.scrollIntoView({ behavior: "smooth" }); return; }
        try {
            setCart(await request<Cart>("/gateway/cart/items", { method: "POST", headers: { "Content-Type": "application/json", ...headers() }, body: JSON.stringify({ productVariantId: item.productVariantId, quantity: 1 }) }));
            setMessage(`${item.productName} added to your cart.`);
        } catch (reason) { showError(reason, "Could not add this recommended product to your cart."); }
    }

    async function handleAuth(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        try {
            const register = authMode === "register";
            const result = await request<AuthResponse>(register ? "/gateway/auth/register" : "/gateway/auth/login", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(register ? { firstName, lastName, email, password } : { email, password }) });
            localStorage.setItem("styleCartToken", result.token); setToken(result.token); setPassword("");
            setMessage(register ? "Account created. You can now shop and check out." : "Signed in successfully. Personalized recommendations are now available.");
        } catch { setError(authMode === "register" ? "Registration failed. Use a unique email and a password with at least 6 characters." : "Login failed. Check your email and password."); }
    }

    function logout() { localStorage.removeItem("styleCartToken"); setToken(""); setOrders([]); setPage("catalog"); setMessage("You have signed out."); }

    async function addToCart() {
        if (!selectedVariantId) return;
        if (!token) { setMessage("Please sign in before adding products to your cart."); document.getElementById("account")?.scrollIntoView({ behavior: "smooth" }); return; }
        try {
            setCart(await request<Cart>("/gateway/cart/items", { method: "POST", headers: { "Content-Type": "application/json", ...headers() }, body: JSON.stringify({ productVariantId: selectedVariantId, quantity }) }));
            setMessage("Product added to your cart.");
        } catch (reason) { showError(reason, "Could not add this product to your cart."); }
    }

    async function changeQuantity(item: CartItem, nextQuantity: number) {
        try { setCart(await request<Cart>(`/gateway/cart/items/${item.id}`, { method: "PUT", headers: { "Content-Type": "application/json", ...headers() }, body: JSON.stringify({ quantity: nextQuantity }) })); }
        catch (reason) { showError(reason, "Could not update this cart item."); }
    }

    async function removeItem(id: number) {
        try { await request<void>(`/gateway/cart/items/${id}`, { method: "DELETE", headers: headers() }); await loadCart(); setMessage("Item removed from your cart."); }
        catch (reason) { showError(reason, "Could not remove this cart item."); }
    }

    async function checkout(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        try {
            const order = await request<Order>("/gateway/orders/checkout", { method: "POST", headers: { "Content-Type": "application/json", ...headers() }, body: JSON.stringify({ recipientName, shippingAddress }) });
            setCart({ id: cart?.id ?? 0, updatedAt: new Date().toISOString(), items: [] }); setOrders((current) => [order, ...current]); setMessage(`Order #${order.id} has been placed successfully.`); setPage("orders");
        } catch (reason) { showError(reason, "Could not place the order."); }
    }

    async function askSupport(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        try { setSupportReply(await request<SupportResponse>("/api/support/chat", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ message: supportMessage }) })); }
        catch { setError("Support chat could not answer right now."); }
    }

    function navigate(nextPage: Page) {
        if (nextPage !== "catalog" && !token) { setError(""); setMessage("Please sign in to use your cart and orders."); document.getElementById("account")?.scrollIntoView({ behavior: "smooth" }); return; }
        setError(""); setPage(nextPage);
        if (nextPage === "cart") void loadCart();
        if (nextPage === "orders") void loadOrders();
    }

    const RecommendationCard = ({ item }: { item: Recommendation }) => (
        <article className="recommendation-card">
            <h3>{item.productName}</h3>
            <p>{item.variantDescription}</p>
            <strong>₹{item.unitPrice}</strong>
            <div className="recommendation-actions">
                <button className="secondary-button" type="button" onClick={() => openRecommendedProduct(item)}>View product</button>
                <button type="button" onClick={() => void addRecommendedProduct(item)}>Add to cart</button>
            </div>
        </article>
    );

    return <main className="app-shell">
        <header className="site-header">
            <div><h1>StyleCart</h1></div>
            <nav className="site-nav">
                <button className="nav-button" onClick={() => navigate("catalog")}>Products</button>
                <button className="nav-button" onClick={() => navigate("cart")}>Cart ({cartItemCount})</button>
                <button className="nav-button" onClick={() => navigate("orders")}>My Orders</button>
                {token ? <button className="secondary-button" onClick={logout}>Logout</button> : <span className="signed-out">Guest shopper</span>}
            </nav>
        </header>

        {message && <p className="success-message toast-message">{message}</p>}
        {error && <p className="error-message">{error}</p>}

        {page === "catalog" && <>
            <section className="hero-section">
                <p className="eyebrow">Fashion catalog</p><h2>Find your next favourite item</h2>
                <p>Search products through the API Gateway, choose a variant, manage your cart, and check out.</p>
                <form className="search-form" onSubmit={handleSearch}>
                    <input type="search" value={searchText} onChange={(event) => setSearchText(event.target.value)} placeholder="Search by name, brand, or description" />
                    <button>Search</button><button className="secondary-button" type="button" onClick={() => { setSearchText(""); void loadProducts(); }}>Clear</button>
                </form>
            </section>

            {!token && <section id="account" className="login-panel">
                <div><p className="eyebrow">Customer account</p><h2>{authMode === "login" ? "Sign in to start shopping" : "Create your customer account"}</h2></div>
                <form className="login-form" onSubmit={handleAuth}>
                    {authMode === "register" && <><input value={firstName} onChange={(event) => setFirstName(event.target.value)} placeholder="First name" required /><input value={lastName} onChange={(event) => setLastName(event.target.value)} placeholder="Last name" required /></>}
                    <input type="email" value={email} onChange={(event) => setEmail(event.target.value)} placeholder="Email" required />
                    <input type="password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Password" minLength={6} required />
                    <button>{authMode === "login" ? "Sign in" : "Create account"}</button>
                    <button className="secondary-button" type="button" onClick={() => setAuthMode(authMode === "login" ? "register" : "login")}>{authMode === "login" ? "New customer? Register" : "Already have an account? Sign in"}</button>
                </form>
            </section>}

            {token && forYou.length > 0 && <section className="recommendation-section"><p className="eyebrow">Personalized picks</p><h2>Recommended for You</h2><div className="recommendation-grid">{forYou.map((item) => <RecommendationCard key={item.productVariantId} item={item} />)}</div></section>}

            <section>
                <div className="section-heading"><div><p className="eyebrow">Products</p><h2>Catalog</h2></div><span>{visibleProducts.length} item(s)</span></div>
                <div className="category-filter" aria-label="Product categories">{categories.map((category) => <button className={selectedCategory === category ? "category-button selected-category" : "category-button"} type="button" key={category} onClick={() => setSelectedCategory(category)}>{category}</button>)}</div>
                {loading && <p>Loading products...</p>}
                <div className="product-grid">{visibleProducts.map((product) => <button className="product-card" key={product.id} onClick={() => void selectProduct(product)}>
                    {product.imageUrl && <img className="product-image" src={product.imageUrl} alt={product.name} />}
                    <p className="eyebrow">{product.category} · {product.brand || "StyleCart"}</p><h3>{product.name}</h3><p>{product.description || "No description available."}</p><strong>₹{product.basePrice}</strong><span className={product.isInStock ? "in-stock" : "out-of-stock"}>{product.isInStock ? "In stock" : "Out of stock"}</span>
                </button>)}</div>
            </section>

            {selectedProduct && <section id="product-details" className="details-panel">
                <button className="close-button" onClick={() => setSelectedProduct(null)}>Close</button>
                <p className="eyebrow">{selectedProduct.brand || "StyleCart"}</p><h2>{selectedProduct.name}</h2><p>{selectedProduct.description || "No description available."}</p><h3>Choose a variant</h3>
                {variants.length === 0 ? <p>No active variants are available for this product.</p> : <>
                    <div className="variant-list">{variants.map((variant) => <label key={variant.id} className={`variant-item ${selectedVariantId === variant.id ? "selected" : ""}`}><input type="radio" name="variant" checked={selectedVariantId === variant.id} disabled={!variant.isInStock} onChange={() => setSelectedVariantId(variant.id)} /><span>{variant.color} · {variant.size}</span><strong>₹{variant.price}</strong><span>{variant.quantityInStock} available</span></label>)}</div>
                    <div className="add-to-cart-row"><label>Quantity <select value={quantity} onChange={(event) => setQuantity(Number(event.target.value))}>{Array.from({ length: 10 }, (_, index) => index + 1).map((amount) => <option key={amount}>{amount}</option>)}</select></label><button type="button" disabled={!selectedVariant?.isInStock} onClick={() => void addToCart()}>Add to cart</button></div>
                </>}
                {alsoBought.length > 0 && <div className="also-bought"><p className="eyebrow">Frequently paired</p><h3>Customers Also Bought</h3><div className="recommendation-grid">{alsoBought.map((item) => <RecommendationCard key={item.productVariantId} item={item} />)}</div></div>}
            </section>}
        </>}

        {page === "cart" && <section className="cart-panel"><div className="section-heading"><div><p className="eyebrow">Your selection</p><h2>Shopping cart</h2></div><button className="secondary-button" onClick={() => navigate("catalog")}>Continue shopping</button></div>{!cart?.items.length ? <p>Your cart is empty.</p> : <><div className="cart-list">{cart.items.map((item) => <article className="cart-item" key={item.id}><div><h3>{item.productName}</h3><p>{item.variantDescription}</p><strong>₹{item.lineTotal}</strong></div><div className="quantity-controls"><button disabled={item.quantity <= 1} onClick={() => void changeQuantity(item, item.quantity - 1)}>−</button><span>{item.quantity}</span><button disabled={item.quantity >= 10} onClick={() => void changeQuantity(item, item.quantity + 1)}>+</button></div><button className="remove-button" onClick={() => void removeItem(item.id)}>Remove</button></article>)}</div><div className="cart-total"><strong>Total: ₹{cartTotal}</strong><button onClick={() => navigate("checkout")}>Checkout</button></div></>}</section>}

        {page === "checkout" && <section className="checkout-panel"><p className="eyebrow">Secure demo checkout</p><h2>Shipping details</h2>{!cart?.items.length ? <p>Your cart is empty.</p> : <form className="checkout-form" onSubmit={checkout}><input value={recipientName} onChange={(event) => setRecipientName(event.target.value)} placeholder="Recipient name" maxLength={150} required /><textarea value={shippingAddress} onChange={(event) => setShippingAddress(event.target.value)} placeholder="Shipping address" maxLength={500} required /><div className="cart-total"><strong>Order total: ₹{cartTotal}</strong><button>Place order</button></div></form>}</section>}

        {page === "orders" && <section className="orders-panel"><div className="section-heading"><div><p className="eyebrow">Purchase history</p><h2>My Orders</h2></div><button className="secondary-button" onClick={() => void loadOrders()}>Refresh</button></div>{loading ? <p>Loading orders...</p> : orders.length === 0 ? <p>You have not placed an order yet.</p> : <div className="orders-list">{orders.map((order) => <article className="order-card" key={order.id}><div><h3>Order #{order.id}</h3><p>{new Date(order.orderDate).toLocaleString()}</p></div><span className="order-status">{order.orderStatus}</span><strong>₹{order.totalAmount}</strong><p>{order.items.map((item) => `${item.productName} (${item.variantDescription}) × ${item.quantity}`).join(", ")}</p></article>)}</div>}</section>}

        <section className="support-panel"><p className="eyebrow">Customer support</p><h2>Ask StyleCart Support</h2><p>Ask about order status, shipping, returns, or refunds.</p><form className="support-form" onSubmit={askSupport}><textarea value={supportMessage} onChange={(event) => setSupportMessage(event.target.value)} placeholder="Example: How can I check my order status?" required /><button>Ask support</button></form>{supportReply && <div className="support-reply"><strong>StyleCart Support</strong><p>{supportReply.reply}</p><small>{supportReply.mode}</small></div>}</section>
    </main>;
}

export default App;
