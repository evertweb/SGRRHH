-- Migration: Add Predio column to Actividades
-- Description: Adds a nullable text column 'predio' to allow categorizing activities by farm/property
-- Author: Antigravity
-- Date: 2026-01-27

ALTER TABLE actividades ADD COLUMN predio TEXT NULL;
