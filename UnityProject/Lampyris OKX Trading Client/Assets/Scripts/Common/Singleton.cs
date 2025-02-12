/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Client.
 * File created at 2024-12-04
*/

namespace HongJinInvestment.OKX.Client.Common
{
    public class Singleton<T> where T : class, new()
    {
        private static T m_instance;

        public static T instance
        {
            get { return m_instance ??= new T(); }
        }
    }
}