import axios from "axios"
import { useRouter } from "vue-router"


function getCookie(name: string) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop()!.split(';').shift();
    return null;
}

const apiClient = axios.create({
    baseURL: 'https://localhost:5133',
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
    }
})

apiClient.interceptors.request.use(
    config => {
        const methodsRequiringXsrf = ['POST', 'PUT', 'DELETE', 'PATCH'];
        if (methodsRequiringXsrf.includes(config.method!.toUpperCase())) {
            const xsrfToken = getCookie('MYAPP-XSRF-TOKEN')
            if (xsrfToken) {
                config.headers['X-XSRF-TOKEN'] = xsrfToken; 
            } else {
                console.warn('XSRF token not found. State-changing request might fail.');
            }
        }
        return config;
    },
    error => {
        return Promise.reject(error);
    }
)

export default apiClient

