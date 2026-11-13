import { useEffect, useState, type FormEvent } from "react";
import "./App.css";

type Product = {
    id: number;
    name: string;
    description: string | null;
    brand: string | null;
    basePrice: number;
    isActive: boolean;
    isInStock: boolean;
};

type Variant = {
    id: number;
    productId: number;
    productName: string;
    size: string;
    color: string;
    sku: string;
    price: number;
    isActive: boolean;
    quantityInStock: number;
    isInStock: boolean;
};

type Recommendation = {
    productVariantId: number;
    productName: string;
    variantDescription: string;
    unitPrice: number;
    purchaseCount: number;
};

type LoginResponse = {
    token: string;
};

type SupportResponse = {
    reply: string;
    mode: string;
};

async function request<T>(
    url: string,
    options: RequestInit = {},
): Promise<T> {
    const response = await fetch(url, options);

    if (!response.ok) {
        const errorBody = await response.text();
        throw new Error(errorBody || `Request failed with status ${response.status}.`);
    }

    return response.json() as Promise<T>;
}

function App() {
    const [products, setProducts] = useState<Product[]>([]);
    const [searchText, setSearchText] = useState("");
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
    const [variants, setVariants] = useState<Variant[]>([]);
    const [alsoBought, setAlsoBought] = useState<Recommendation[]>([]);
    const [forYou, setForYou] = useState<Recommendation[]>([]);
    const [supportMessage, setSupportMessage] = useState("");
    const [supportReply, setSupportReply] = useState<SupportResponse | null>(null);
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [token, setToken] = useState(localStorage.getItem("styleCartToken") ?? "");
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    useEffect(() => {
        void loadProducts();
    }, []);

    useEffect(() => {
        if (token) {
            void loadForYou();
        } else {
            setForYou([]);
        }
    }, [token]);

    async function loadProducts(search = "") {
        try {
            setLoading(true);
            setError("");

            const query = search ? `?search=${encodeURIComponent(search)}` : "";
            const data = await request<Product[]>(`/gateway/products${query}`);

            setProducts(data);
        } catch (requestError) {
            setError(
                requestError instanceof Error
                    ? requestError.message
                    : "Could not load products.",
            );
        } finally {
            setLoading(false);
        }
    }

    async function loadForYou() {
        try {
            const data = await request<Recommendation[]>(
                "/gateway/recommendations/for-you",
                {
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                },
            );

            setForYou(data);
        } catch {
            setForYou([]);
        }
    }

    async function handleSearch(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        const search = searchText.trim();

        if (search.length === 1) {
            setError("Enter at least two characters to search.");
            return;
        }

        await loadProducts(search);
    }

    async function selectProduct(product: Product) {
        try {
            setSelectedProduct(product);
            setVariants([]);
            setAlsoBought([]);
            setError("");

            const variantData = await request<Variant[]>(
                `/gateway/products/${product.id}/variants`,
            );

            const firstVariantId = variantData[0]?.id;
            const recommendationData = firstVariantId
                ? await request<Recommendation[]>(
                    `/gateway/recommendations/variants/${firstVariantId}/also-bought`,
                )
                : [];

            setVariants(variantData);
            setAlsoBought(recommendationData);
        } catch (requestError) {
            setError(
                requestError instanceof Error
                    ? requestError.message
                    : "Could not load product details.",
            );
        }
    }

    async function handleLogin(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        try {
            setError("");

            const result = await request<LoginResponse>("/gateway/auth/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ email, password }),
            });

            localStorage.setItem("styleCartToken", result.token);
            setToken(result.token);
            setPassword("");
            setMessage("Signed in successfully. Personalized recommendations are now available.");
        } catch {
            setError("Login failed. Check your email and password.");
        }
    }

    function handleLogout() {
        localStorage.removeItem("styleCartToken");
        setToken("");
        setForYou([]);
        setMessage("You have signed out.");
    }

    async function handleSupport(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        try {
            setError("");
            setSupportReply(null);

            const result = await request<SupportResponse>("/api/support/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ message: supportMessage }),
            });

            setSupportReply(result);
        } catch {
            setError("Support chat could not answer right now.");
        }
    }

    return (
        <main className= "app-shell" >
        <header className="site-header" >
            <div>
            <p className="eyebrow" > Microservices demo </p>
                < h1 > StyleCart </h1>
                </div>

    {
        token ? (
            <button className= "secondary-button" type = "button" onClick = { handleLogout } >
                Logout
                </button>
                ) : (
            <span className= "signed-out" > Guest shopper </span>
                )
    }
    </header>

    { message && <p className="success-message" > { message } </p> }
    { error && <p className="error-message" > { error } </p> }

    <section className="hero-section" >
        <p className="eyebrow" > Fashion catalog </p>
            < h2 > Find your next favourite item </h2>
                <p>
                    Search products through the API Gateway, explore variants,
        and ask the support assistant for help.
                </p>

            < form className = "search-form" onSubmit = { handleSearch } >
            <input
                        type= "search"
    value = { searchText }
    onChange = {(event) => setSearchText(event.target.value)
}
placeholder = "Search by name, brand, or description"
    />
    <button type="submit" > Search </button>
        < button
className = "secondary-button"
type = "button"
onClick = {() => {
    setSearchText("");
    void loadProducts();
}}
                    >
    Clear
    </button>
    </form>
    </section>

{
    !token && (
        <section className="login-panel" >
            <div>
            <p className="eyebrow" > Customer account </p>
                < h2 > Sign in for personalized recommendations </h2>
                    </div>

                    < form className = "login-form" onSubmit = { handleLogin } >
                        <input
                            type="email"
    value = { email }
    onChange = {(event) => setEmail(event.target.value)
}
placeholder = "Email"
required
    />
    <input
                            type="password"
value = { password }
onChange = {(event) => setPassword(event.target.value)}
placeholder = "Password"
required
    />
    <button type="submit" > Sign in </button>
        </form>
        </section>
            )}

{
    token && forYou.length > 0 && (
        <section className="recommendation-section" >
            <p className="eyebrow" > Personalized picks </p>
                < h2 > Recommended for You </h2>
                    < div className = "recommendation-grid" >
                    {
                        forYou.map((item) => (
                            <article key= { item.productVariantId } className = "recommendation-card" >
                            <h3>{ item.productName } </h3>
                            < p > { item.variantDescription } </p>
                            <strong>₹{ item.unitPrice } </strong>
                        </article>
                        ))
                    }
                    </div>
                    </section>
            )}

<section>
    <div className="section-heading" >
        <div>
        <p className="eyebrow" > Products </p>
            < h2 > Catalog </h2>
            </div>
            < span > { products.length } item(s) </span>
                </div>

{ loading && <p>Loading products...</p> }

<div className="product-grid" >
{
    products.map((product) => (
        <button
                            className= "product-card"
                            key = { product.id }
                            type = "button"
                            onClick = {() => void selectProduct(product)}
    >
    <p className="eyebrow" > { product.brand || "StyleCart" } </p>
        < h3 > { product.name } </h3>
        < p > { product.description || "No description available." } </p>
        <strong>₹{ product.basePrice } </strong>
            < span className = { product.isInStock ? "in-stock" : "out-of-stock" } >
            { product.isInStock ? "In stock" : "Out of stock" }
                </span>
                </button>
                    ))}
</div>
    </section>

{
    selectedProduct && (
        <section className="details-panel" >
            <button
                        className="close-button"
    type = "button"
    onClick = {() => setSelectedProduct(null)
}
                    >
    Close
    </button>

    < p className = "eyebrow" > { selectedProduct.brand || "StyleCart" } </p>
        < h2 > { selectedProduct.name } </h2>
        < p > { selectedProduct.description || "No description available." } </p>

        < h3 > Available variants </h3>
{
    variants.length === 0 ? (
        <p>No active variants are available for this product.</p>
                    ) : (
            <div className= "variant-list" >
            {
                variants.map((variant) => (
                    <div key= { variant.id } className = "variant-item" >
                    <span>
                    { variant.color } · { variant.size }
                </span>
                <strong>₹{ variant.price } </strong>
                < span > { variant.quantityInStock } available </span>
                </div>
                ))
            }
            </div>
                    )
}

{
    alsoBought.length > 0 && (
        <div className="also-bought" >
            <p className="eyebrow" > Frequently paired </p>
                < h3 > Customers Also Bought </h3>
                    < div className = "recommendation-grid" >
                    {
                        alsoBought.map((item) => (
                            <article
                                        key= { item.productVariantId }
                                        className = "recommendation-card"
                            >
                            <h4>{ item.productName } </h4>
                            < p > { item.variantDescription } </p>
                            <strong>₹{ item.unitPrice } </strong>
                        </article>
                        ))
                    }
                        </div>
                        </div>
                    )
}
</section>
            )}

<section className="support-panel" >
    <p className="eyebrow" > Customer support </p>
        < h2 > Ask StyleCart Support </h2>
            < p > Ask about order status, shipping, returns, or refunds.</p>

                < form className = "support-form" onSubmit = { handleSupport } >
                    <textarea
                        value={ supportMessage }
onChange = {(event) => setSupportMessage(event.target.value)}
placeholder = "Example: How can I check my order status?"
required
    />
    <button type="submit" > Ask support </button>
        </form>

{
    supportReply && (
        <div className="support-reply" >
            <strong>StyleCart Support </strong>
                < p > { supportReply.reply } </p>
                < small > { supportReply.mode } </small>
                </div>
                )
}
</section>
    </main>
    );
}

export default App;
