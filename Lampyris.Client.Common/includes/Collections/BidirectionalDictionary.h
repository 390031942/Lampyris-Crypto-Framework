#include <unordered_map>
#include <stdexcept>
#include <iostream>
#include <vector>

template <typename TKey, typename TValue>
class BidirectionalDictionary {
private:
    std::unordered_map<TKey, TValue> forward;  // 正向映射：键 -> 值
    std::unordered_map<TValue, TKey> reverse;  // 反向映射：值 -> 键

public:

    // 支持集合初始化的构造函数
    BidirectionalDictionary(std::initializer_list<std::pair<TKey, TValue>> list) {
        for (const auto& pair : list) {
            add(pair.first, pair.second);
        }
    }

    // 添加键值对
    void add(const TKey& key, const TValue& value) {
        if (forward.count(key)) {
            throw std::invalid_argument("Key already exists in the dictionary.");
        }
        if (reverse.count(value)) {
            throw std::invalid_argument("Value already exists in the dictionary.");
        }
        forward[key] = value;
        reverse[value] = key;
    }

    // 根据键查找值
    bool tryGetByKey(const TKey& key, TValue& value) const {
        auto it = forward.find(key);
        if (it != forward.end()) {
            value = it->second;
            return true;
        }
        return false;
    }

    // 根据值查找键
    bool tryGetByValue(const TValue& value, TKey& key) const {
        auto it = reverse.find(value);
        if (it != reverse.end()) {
            key = it->second;
            return true;
        }
        return false;
    }

    // 获取值（通过键）
    TValue getByKey(const TKey& key) const {
        auto it = forward.find(key);
        if (it == forward.end()) {
            throw std::out_of_range("Key not found in the dictionary.");
        }
        return it->second;
    }

    // 获取键（通过值）
    TKey getByValue(const TValue& value) const {
        auto it = reverse.find(value);
        if (it == reverse.end()) {
            throw std::out_of_range("Value not found in the dictionary.");
        }
        return it->second;
    }

    // 删除键值对（通过键）
    void removeByKey(const TKey& key) {
        auto it = forward.find(key);
        if (it == forward.end()) {
            throw std::out_of_range("Key not found in the dictionary.");
        }
        TValue value = it->second;
        forward.erase(it);
        reverse.erase(value);
    }

    // 删除键值对（通过值）
    void removeByValue(const TValue& value) {
        auto it = reverse.find(value);
        if (it == reverse.end()) {
            throw std::out_of_range("Value not found in the dictionary.");
        }
        TKey key = it->second;
        reverse.erase(it);
        forward.erase(key);
    }

    // 获取字典中的键数量
    size_t count() const {
        return forward.size();
    }

    // 获取所有键
    std::vector<TKey> keys() const {
        std::vector<TKey> result;
        for (const auto& pair : forward) {
            result.push_back(pair.first);
        }
        return result;
    }

    // 获取所有值
    std::vector<TValue> values() const {
        std::vector<TValue> result;
        for (const auto& pair : reverse) {
            result.push_back(pair.first);
        }
        return result;
    }

    // 清空字典
    void clear() {
        forward.clear();
        reverse.clear();
    }

    // 索引器：通过键访问值
    TValue operator[](const TKey& key) const {
        return getByKey(key);
    }
};