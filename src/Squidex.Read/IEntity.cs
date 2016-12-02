﻿// ==========================================================================
//  IEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Read
{
    public interface IEntity
    {
        Guid Id { get; }

        DateTime Created { get; }

        DateTime LastModified { get; }
    }
}