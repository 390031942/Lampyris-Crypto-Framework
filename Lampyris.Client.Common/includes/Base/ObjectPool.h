// STD Include(s)
#include <vector>
#include <memory>
#include <mutex>
#include <functional>
#include <stdexcept>

// 对象池模板类
template <typename T>
class ObjectPool {
public:
    // 构造函数，指定池的大小和对象的创建函数
    explicit ObjectPool(size_t poolSize, std::function<std::shared_ptr<T>()> objectCreator = []() { return std::make_shared<T>(); })
        : m_poolSize(poolSize), m_objectCreator(objectCreator) {
        if (poolSize == 0) {
            throw std::invalid_argument("Pool size must be greater than 0");
        }
        initializePool();
    }

    // 获取一个对象
    std::shared_ptr<T> get() {
        std::lock_guard<std::mutex> lock(m_mutex);

        if (!m_availableObjects.empty()) {
            // 从可用对象列表中取出一个对象
            auto obj = m_availableObjects.back();
            m_availableObjects.pop_back();
            return obj;
        }

        // 如果没有可用对象，尝试创建一个新的对象（如果允许）
        if (m_activeObjects.size() < m_poolSize) {
            auto obj = m_objectCreator();
            m_activeObjects.push_back(obj);
            return obj;
        }

        // 如果池已满，抛出异常或返回空指针（根据需求调整）
        throw std::runtime_error("No available objects in the pool");
    }

    // 释放一个对象，将其归还到池中
    void recycle(std::shared_ptr<T> obj) {
        std::lock_guard<std::mutex> lock(m_mutex);

        // 检查对象是否属于池
        auto it = std::find(m_activeObjects.begin(), m_activeObjects.end(), obj);
        if (it != m_activeObjects.end()) {
            m_availableObjects.push_back(obj);
        }
        else {
            throw std::invalid_argument("Object does not belong to this pool");
        }
    }

    // 获取池中可用对象的数量
    size_t availableCount() const {
        std::lock_guard<std::mutex> lock(m_mutex);
        return m_availableObjects.size();
    }

    // 获取池中总对象的数量
    size_t totalCount() const {
        return m_poolSize;
    }

private:
    size_t m_poolSize;                                 // 对象池的最大大小
    std::function<std::shared_ptr<T>()> m_objectCreator; // 对象创建函数
    std::vector<std::shared_ptr<T>> m_activeObjects;  // 所有活跃的对象
    std::vector<std::shared_ptr<T>> m_availableObjects; // 可用对象列表
    mutable std::mutex m_mutex;                       // 线程安全的互斥锁

    // 初始化对象池
    void initializePool() {
        for (size_t i = 0; i < m_poolSize; ++i) {
            auto obj = m_objectCreator();
            m_availableObjects.push_back(obj);
            m_activeObjects.push_back(obj);
        }
    }
};
