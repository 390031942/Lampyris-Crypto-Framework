// STD Include(s)
#include <vector>
#include <memory>
#include <mutex>
#include <functional>
#include <stdexcept>

// �����ģ����
template <typename T>
class ObjectPool {
public:
    // ���캯����ָ���صĴ�С�Ͷ���Ĵ�������
    explicit ObjectPool(size_t poolSize, std::function<std::shared_ptr<T>()> objectCreator = []() { return std::make_shared<T>(); })
        : m_poolSize(poolSize), m_objectCreator(objectCreator) {
        if (poolSize == 0) {
            throw std::invalid_argument("Pool size must be greater than 0");
        }
        initializePool();
    }

    // ��ȡһ������
    std::shared_ptr<T> get() {
        std::lock_guard<std::mutex> lock(m_mutex);

        if (!m_availableObjects.empty()) {
            // �ӿ��ö����б���ȡ��һ������
            auto obj = m_availableObjects.back();
            m_availableObjects.pop_back();
            return obj;
        }

        // ���û�п��ö��󣬳��Դ���һ���µĶ����������
        if (m_activeObjects.size() < m_poolSize) {
            auto obj = m_objectCreator();
            m_activeObjects.push_back(obj);
            return obj;
        }

        // ������������׳��쳣�򷵻ؿ�ָ�루�������������
        throw std::runtime_error("No available objects in the pool");
    }

    // �ͷ�һ�����󣬽���黹������
    void recycle(std::shared_ptr<T> obj) {
        std::lock_guard<std::mutex> lock(m_mutex);

        // �������Ƿ����ڳ�
        auto it = std::find(m_activeObjects.begin(), m_activeObjects.end(), obj);
        if (it != m_activeObjects.end()) {
            m_availableObjects.push_back(obj);
        }
        else {
            throw std::invalid_argument("Object does not belong to this pool");
        }
    }

    // ��ȡ���п��ö��������
    size_t availableCount() const {
        std::lock_guard<std::mutex> lock(m_mutex);
        return m_availableObjects.size();
    }

    // ��ȡ�����ܶ��������
    size_t totalCount() const {
        return m_poolSize;
    }

private:
    size_t m_poolSize;                                 // ����ص�����С
    std::function<std::shared_ptr<T>()> m_objectCreator; // ���󴴽�����
    std::vector<std::shared_ptr<T>> m_activeObjects;  // ���л�Ծ�Ķ���
    std::vector<std::shared_ptr<T>> m_availableObjects; // ���ö����б�
    mutable std::mutex m_mutex;                       // �̰߳�ȫ�Ļ�����

    // ��ʼ�������
    void initializePool() {
        for (size_t i = 0; i < m_poolSize; ++i) {
            auto obj = m_objectCreator();
            m_availableObjects.push_back(obj);
            m_activeObjects.push_back(obj);
        }
    }
};
